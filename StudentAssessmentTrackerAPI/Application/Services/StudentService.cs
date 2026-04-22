using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;
using AutoMapper;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>
    /// Defines the contract for student data access (teacher-scoped) and student
    /// self-service (account activation and login) operations.
    /// All write operations (create, update, delete, assign) are admin-only and live in IAdminService.
    /// </summary>
    public interface IStudentService
    {
        /// <summary>Returns all students that belong to the specified teacher.</summary>
        Task<IEnumerable<StudentDto>> GetAllStudentsAsync(int teacherId);

        /// <summary>
        /// Returns the student with <paramref name="id"/> if it belongs to
        /// <paramref name="teacherId"/>; throws <see cref="KeyNotFoundException"/> otherwise.
        /// </summary>
        Task<StudentDto> GetStudentByIdAsync(int id, int teacherId);

        /// <summary>
        /// Activates a student account with an initial password.
        /// Returns <c>null</c> when the UniqueId/email combination does not match.
        /// Throws <see cref="InvalidOperationException"/> when the account is already active.
        /// </summary>
        Task<StudentLoginResponseDto?> ActivateStudentAsync(StudentActivateDto dto);

        /// <summary>
        /// Authenticates a student by UniqueId and password.
        /// Returns <c>null</c> on credential mismatch.
        /// Throws <see cref="InvalidOperationException"/> when the account has not been activated.
        /// </summary>
        Task<StudentLoginResponseDto?> LoginStudentAsync(StudentLoginDto dto);

        /// <summary>
        /// Resets a student's password by nulling it so they must re-activate.
        /// Both <paramref name="dto"/>.StudentUniqueId and <paramref name="dto"/>.Email must match the record.
        /// Throws <see cref="KeyNotFoundException"/> when no matching student is found.
        /// </summary>
        Task ForgotPasswordAsync(StudentForgotPasswordDto dto);
    }

    /// <summary>
    /// Handles student CRUD, teacher-scoped data access, and student self-service
    /// (account activation and login) operations.
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>Initialises the service with its dependencies.</summary>
        public StudentService(
            IStudentRepository repository,
            IMapper mapper,
            ILogger<StudentService> logger,
            IConfiguration configuration)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync(int teacherId)
        {
            _logger.LogInformation("Fetching students for teacher {TeacherId}", teacherId);
            var students = await _repository.GetAllByTeacherAsync(teacherId);
            return _mapper.Map<IEnumerable<StudentDto>>(students);
        }

        /// <inheritdoc />
        public async Task<StudentDto> GetStudentByIdAsync(int id, int teacherId)
        {
            _logger.LogInformation("Fetching student {StudentId} for teacher {TeacherId}", id, teacherId);
            var student = await _repository.GetByIdForTeacherAsync(id, teacherId);
            if (student == null)
                throw new KeyNotFoundException($"Student with ID {id} not found.");
            return _mapper.Map<StudentDto>(student);
        }

        /// <inheritdoc />
        public async Task<StudentLoginResponseDto?> ActivateStudentAsync(StudentActivateDto dto)
        {
            _logger.LogInformation("Activation attempt for StudentUniqueId {UniqueId}", dto.StudentUniqueId);
            var student = await _repository.FindByUniqueIdAsync(dto.StudentUniqueId);
            if (student is null ||
                !student.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Activation failed - no match for UniqueId {UniqueId}", dto.StudentUniqueId);
                return null;
            }
            if (!string.IsNullOrEmpty(student.Password))
                throw new InvalidOperationException("This account has already been activated. Please use the login page.");
            // OWASP A02: hash the password with BCrypt before storage — same as Teacher and Admin.
            student.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            student.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(student);
            var fullStudent = await _repository.GetByIdAsync(student.Id) ?? student;
            return new StudentLoginResponseDto
            {
                Token = GenerateStudentJwtToken(student),
                Student = _mapper.Map<StudentProfileDto>(fullStudent)
            };
        }

        /// <inheritdoc />
        public async Task<StudentLoginResponseDto?> LoginStudentAsync(StudentLoginDto dto)
        {
            _logger.LogInformation("Student login attempt for {UniqueId}", dto.StudentUniqueId);
            var student = await _repository.FindByUniqueIdAsync(dto.StudentUniqueId);
            if (student is null)
            {
                _logger.LogWarning("Student login failed - UniqueId {UniqueId} not found", dto.StudentUniqueId);
                return null;
            }
            if (string.IsNullOrEmpty(student.Password))
                throw new InvalidOperationException("Account not activated. Please use the sign-up page to activate your account first.");
            // OWASP A02: verify with BCrypt — never compare passwords as plaintext strings.
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, student.Password))
            {
                _logger.LogWarning("Student login failed - wrong password for {UniqueId}", dto.StudentUniqueId);
                return null;
            }
            var fullStudent = await _repository.GetByIdAsync(student.Id) ?? student;
            return new StudentLoginResponseDto
            {
                Token = GenerateStudentJwtToken(student),
                Student = _mapper.Map<StudentProfileDto>(fullStudent)
            };
        }

        /// <inheritdoc />
        public async Task ForgotPasswordAsync(StudentForgotPasswordDto dto)
        {
            var student = await _repository.FindByUniqueIdAsync(dto.StudentUniqueId);
            if (student is null ||
                !student.Email.Equals(dto.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                throw new KeyNotFoundException("No student found with that Student ID and email combination.");

            student.Password = null;
            student.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(student);

            _logger.LogInformation("Student {StudentId} password reset via forgot-password", student.Id);
        }

        private string GenerateStudentJwtToken(Student student)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
            var signingKey = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var expiresMinutes = double.TryParse(_configuration["Jwt:ExpiresInMinutes"], out var m) ? m : 480;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   student.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, student.Email),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                // Custom claims — StudentAssessmentsController reads studentId to scope queries
                new Claim("studentId", student.Id.ToString()),
                new Claim(ClaimTypes.Role, "Student"),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
