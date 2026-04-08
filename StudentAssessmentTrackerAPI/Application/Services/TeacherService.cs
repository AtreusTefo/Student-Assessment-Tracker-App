using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>
    /// Defines the contract for teacher account management and authentication operations.
    /// </summary>
    public interface ITeacherService
    {
        /// <summary>Returns all registered teachers.</summary>
        Task<IEnumerable<TeacherResponseDto>> GetAllTeachersAsync();

        /// <summary>Returns the teacher with <paramref name="id"/>, or <c>null</c> if not found.</summary>
        Task<TeacherResponseDto?> GetTeacherByIdAsync(int id);

        /// <summary>
        /// Creates a new teacher account.
        /// Throws <see cref="InvalidOperationException"/> on duplicate email or ID/Passport number.
        /// </summary>
        Task<TeacherResponseDto> CreateTeacherAsync(TeacherRegisterDto dto);

        /// <summary>
        /// Updates an existing teacher. Returns <c>false</c> when the teacher is not found.
        /// Throws <see cref="ArgumentException"/> when SubjectId is invalid.
        /// Throws <see cref="InvalidOperationException"/> on duplicate email or ID/Passport number.
        /// </summary>
        Task<bool> UpdateTeacherAsync(int id, TeacherUpdateDto dto);

        /// <summary>
        /// Deletes a teacher. Returns <c>false</c> when not found.
        /// Throws <see cref="InvalidOperationException"/> when the teacher still has students assigned.
        /// </summary>
        Task<bool> DeleteTeacherAsync(int id);

        /// <summary>
        /// Authenticates a teacher by email and password and returns a signed JWT on success.
        /// Returns <c>null</c> on credential mismatch.
        /// </summary>
        Task<TeacherLoginResponseDto?> LoginAsync(TeacherLoginDto dto);
    }

    /// <summary>
    /// Handles teacher CRUD, duplicate-detection, FK-safe deletion, and JWT-based authentication.
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _repository;
        private readonly IRepository<Subject> _subjectRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>Initialises the service with the teacher repository, subject repository, mapper, logger, and configuration.</summary>
        public TeacherService(
            ITeacherRepository repository,
            IRepository<Subject> subjectRepository,
            IMapper mapper,
            ILogger<TeacherService> logger,
            IConfiguration configuration)
        {
            _repository = repository;
            _subjectRepository = subjectRepository;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TeacherResponseDto>> GetAllTeachersAsync()
        {
            _logger.LogInformation("Retrieving all teachers");
            var teachers = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TeacherResponseDto>>(teachers);
        }

        /// <inheritdoc />
        public async Task<TeacherResponseDto?> GetTeacherByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving teacher with ID {TeacherId}", id);
            var teacher = await _repository.GetByIdAsync(id);
            return teacher is null ? null : _mapper.Map<TeacherResponseDto>(teacher);
        }

        /// <inheritdoc />
        public async Task<TeacherResponseDto> CreateTeacherAsync(TeacherRegisterDto dto)
        {
            _logger.LogInformation("Creating teacher with email {Email}", dto.Email);

            // Validate SubjectId against the Subjects table before any other checks.
            if (await _subjectRepository.GetByIdAsync(dto.SubjectId) is null)
                throw new ArgumentException($"Subject with ID {dto.SubjectId} does not exist.");

            // Issue 4: detect duplicate email before hitting the unique DB index.
            var existing = await _repository.FindByEmailAsync(dto.Email);
            if (existing is not null)
                throw new InvalidOperationException($"A teacher with email '{dto.Email}' is already registered.");

            // Issue 4: detect duplicate ID/Passport number.
            if (await _repository.ExistsByIdPassportNoAsync(dto.IdPassportNo))
                throw new InvalidOperationException($"A teacher with ID/Passport No. '{dto.IdPassportNo}' is already registered.");

            var teacher = _mapper.Map<Teacher>(dto);
            teacher.CreatedDate = DateTime.UtcNow;
            // Issue 5: AddAsync already calls SaveChangesAsync internally — do not call it again.
            await _repository.AddAsync(teacher);
            return _mapper.Map<TeacherResponseDto>(teacher);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateTeacherAsync(int id, TeacherUpdateDto dto)
        {
            _logger.LogInformation("Updating teacher with ID {TeacherId}", id);
            var teacher = await _repository.GetByIdAsync(id);
            if (teacher is null) return false;

            // Validate SubjectId against the Subjects table.
            if (await _subjectRepository.GetByIdAsync(dto.SubjectId) is null)
                throw new ArgumentException($"Subject with ID {dto.SubjectId} does not exist.");

            // Issue 2 fix: if the teacher is changing their subject and they have students
            // assigned, the existing TeacherStudent.SubjectId rows would become stale, silently
            // corrupting the one-teacher-per-subject-per-student invariant enforced by the
            // UX_TeacherStudents_StudentId_SubjectId unique index.  Block until all students
            // are unassigned first.
            if (dto.SubjectId != teacher.SubjectId && await _repository.HasStudentsAsync(id))
                throw new InvalidOperationException(
                    $"Cannot change subject: teacher {id} still has students assigned. " +
                    "Unassign all students first, then update the subject.");

            // Detect duplicate email, excluding the record being updated.
            if (await _repository.ExistsByEmailAsync(dto.Email, excludeTeacherId: id))
                throw new InvalidOperationException($"A teacher with email '{dto.Email}' is already registered.");

            // Detect duplicate ID/Passport number, excluding the record being updated.
            if (await _repository.ExistsByIdPassportNoAsync(dto.IdPassportNo, excludeTeacherId: id))
                throw new InvalidOperationException($"A teacher with ID/Passport No. '{dto.IdPassportNo}' is already registered.");

            _mapper.Map(dto, teacher);
            await _repository.UpdateAsync(teacher);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTeacherAsync(int id)
        {
            _logger.LogInformation("Deleting teacher with ID {TeacherId}", id);
            var teacher = await _repository.GetByIdAsync(id);
            if (teacher is null) return false;

            // Issue 3: pre-check the FK RESTRICT constraint instead of letting the DB
            // throw a DbUpdateException that would surface as an opaque 500.
            if (await _repository.HasStudentsAsync(id))
                throw new InvalidOperationException(
                    $"Teacher with ID {id} cannot be deleted because they have students assigned. " +
                    "Reassign or delete the students first.");

            // DeleteAsync already calls SaveChangesAsync internally — no second call needed.
            await _repository.DeleteAsync(id);
            return true;
        }

        /// <summary>
        /// Authenticates a teacher by email using a single indexed DB query (no full-table
        /// scan) and returns a signed JWT containing the teacher's ID as a claim.
        /// </summary>
        public async Task<TeacherLoginResponseDto?> LoginAsync(TeacherLoginDto dto)
        {
            _logger.LogInformation("Login attempt for email {Email}", dto.Email);

            // Single server-side query â€” no GetAllAsync() table scan
            var teacher = await _repository.FindByEmailAsync(dto.Email);

            if (teacher is null || teacher.Password != dto.Password)
            {
                _logger.LogWarning("Failed login attempt for email {Email}", dto.Email);
                return null;
            }

            return new TeacherLoginResponseDto
            {
                Token = GenerateJwtToken(teacher),
                Teacher = _mapper.Map<TeacherResponseDto>(teacher)
            };
        }

        // â”€â”€ Private helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private string GenerateJwtToken(Teacher teacher)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
            var signingKey = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var expiresMinutes = double.TryParse(_configuration["Jwt:ExpiresInMinutes"], out var m) ? m : 480;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   teacher.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, teacher.Email),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                // Custom claim — controllers read this to scope data queries
                new Claim("teacherId", teacher.Id.ToString()),
                new Claim(System.Security.Claims.ClaimTypes.Role, "Teacher"),
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
