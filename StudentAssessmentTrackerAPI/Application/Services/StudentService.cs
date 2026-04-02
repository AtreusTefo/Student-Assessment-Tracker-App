using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>
    /// Defines the contract for student CRUD, teacher-scoped data access, and student
    /// self-service (account activation and login) operations.
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
        /// Creates a new student under <paramref name="teacherId"/>.
        /// Throws <see cref="KeyNotFoundException"/> when the teacher does not exist,
        /// <see cref="ArgumentException"/> when the grade is invalid, and
        /// <see cref="InvalidOperationException"/> on duplicate email or ID/Passport.
        /// </summary>
        Task<StudentDto> CreateStudentAsync(CreateStudentDto dto, int teacherId);

        /// <summary>
        /// Updates an existing student owned by <paramref name="teacherId"/>.
        /// Throws <see cref="KeyNotFoundException"/> when not found,
        /// <see cref="ArgumentException"/> when the grade is invalid, and
        /// <see cref="InvalidOperationException"/> on duplicate email or ID/Passport.
        /// </summary>
        Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentDto dto, int teacherId);

        /// <summary>
        /// Deletes the student with <paramref name="id"/> owned by <paramref name="teacherId"/>.
        /// Throws <see cref="KeyNotFoundException"/> when not found.
        /// </summary>
        Task DeleteStudentAsync(int id, int teacherId);

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
        /// Assigns <paramref name="teacherId"/> to <paramref name="studentId"/> in the join table.
        /// Idempotent — calling it twice has no effect and does not throw.
        /// Throws <see cref="KeyNotFoundException"/> when either entity does not exist.
        /// </summary>
        Task AssignStudentToTeacherAsync(int studentId, int teacherId);

        /// <summary>
        /// Removes the assignment between <paramref name="teacherId"/> and <paramref name="studentId"/>.
        /// Throws <see cref="KeyNotFoundException"/> when the assignment does not exist.
        /// </summary>
        Task UnassignStudentFromTeacherAsync(int studentId, int teacherId);
    }

    /// <summary>
    /// Handles student CRUD, teacher-scoped data access, and student self-service
    /// (account activation and login) operations.
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IRepository<Grade> _gradeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>Initialises the service with all required repositories, mapper, logger, and configuration.</summary>
        public StudentService(
            IStudentRepository repository,
            ITeacherRepository teacherRepository,
            IRepository<Grade> gradeRepository,
            IMapper mapper,
            ILogger<StudentService> logger,
            IConfiguration configuration)
        {
            _repository = repository;
            _teacherRepository = teacherRepository;
            _gradeRepository = gradeRepository;
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
        public async Task<StudentDto> CreateStudentAsync(CreateStudentDto dto, int teacherId)
        {
            _logger.LogInformation("Creating new student: {FirstName} {LastName}", dto.FirstName, dto.LastName);

            // Issue 1: verify the teacher from the JWT claim actually exists in the DB.
            // A valid (or replayed) token with a deleted teacher ID would otherwise hit
            // the FK constraint and surface as an opaque 500.
            var teacherExists = await _teacherRepository.GetByIdAsync(teacherId);
            if (teacherExists is null)
                throw new KeyNotFoundException($"Teacher with ID {teacherId} no longer exists.");

            // Issue 2: validate GradeId against the Grades lookup table.
            var gradeExists = await _gradeRepository.GetByIdAsync(dto.GradeId);
            if (gradeExists is null)
                throw new ArgumentException($"Grade with ID {dto.GradeId} does not exist.");

            // Issue 4: detect duplicate email before hitting the unique DB index.
            if (await _repository.ExistsByEmailAsync(dto.Email!))
                throw new InvalidOperationException($"A student with email '{dto.Email}' is already registered.");

            // Issue 4: detect duplicate ID/Passport number.
            if (await _repository.ExistsByIdPassportNoAsync(dto.IdPassportNo!))
                throw new InvalidOperationException($"A student with ID/Passport No. '{dto.IdPassportNo}' is already registered.");

            var student = _mapper.Map<Student>(dto);
            // Retry until we get a UniqueId that is not already taken in the DB.
            string uniqueId;
            do
            {
                uniqueId = GenerateStudentUniqueId();
            } while (await _repository.FindByUniqueIdAsync(uniqueId) is not null);
            student.StudentUniqueId = uniqueId;
            student.CreatedAt = DateTime.UtcNow;
            student.UpdatedAt = DateTime.UtcNow;

            // Atomic: INSERT student row + TeacherStudents join row in one SaveChangesAsync
            // call so that a failure on either side never leaves an orphaned student.
            await _repository.AddWithTeacherAssignmentAsync(student, teacherId);

            _logger.LogInformation("Student created with ID {StudentId}, UniqueId {UniqueId}, assigned to teacher {TeacherId}",
                student.Id, student.StudentUniqueId, teacherId);
            return _mapper.Map<StudentDto>(student);
        }

        /// <inheritdoc />
        public async Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentDto dto, int teacherId)
        {
            _logger.LogInformation("Updating student {StudentId} for teacher {TeacherId}", id, teacherId);
            var student = await _repository.GetByIdForTeacherAsync(id, teacherId);
            if (student == null)
                throw new KeyNotFoundException($"Student with ID {id} not found.");

            // Issue 2: validate GradeId against the Grades lookup table.
            var gradeExists = await _gradeRepository.GetByIdAsync(dto.GradeId);
            if (gradeExists is null)
                throw new ArgumentException($"Grade with ID {dto.GradeId} does not exist.");

            // Issue 4: detect duplicate email, excluding the student being updated.
            if (await _repository.ExistsByEmailAsync(dto.Email!, excludeStudentId: id))
                throw new InvalidOperationException($"A student with email '{dto.Email}' is already registered.");

            // Issue 4: detect duplicate ID/Passport number, excluding the student being updated.
            if (await _repository.ExistsByIdPassportNoAsync(dto.IdPassportNo!, excludeStudentId: id))
                throw new InvalidOperationException($"A student with ID/Passport No. '{dto.IdPassportNo}' is already registered.");

            _mapper.Map(dto, student);
            student.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(student);
            return _mapper.Map<StudentDto>(student);
        }

        /// <inheritdoc />
        public async Task DeleteStudentAsync(int id, int teacherId)
        {
            _logger.LogInformation("Deleting student {StudentId} for teacher {TeacherId}", id, teacherId);
            var student = await _repository.GetByIdForTeacherAsync(id, teacherId);
            if (student == null)
                throw new KeyNotFoundException($"Student with ID {id} not found.");
            await _repository.DeleteAsync(id);
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
            student.Password = dto.Password;
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
            if (student.Password != dto.Password)
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

        private static string GenerateStudentUniqueId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            // Random.Shared is thread-safe (unlike new Random() per call).
            var suffix = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
            return $"STU-{suffix}";
        }

        /// <inheritdoc />
        public async Task AssignStudentToTeacherAsync(int studentId, int teacherId)
        {
            _logger.LogInformation("Assigning teacher {TeacherId} to student {StudentId}", teacherId, studentId);

            // Both sides must exist before inserting the join row
            var teacher = await _teacherRepository.GetByIdAsync(teacherId);
            if (teacher is null)
                throw new KeyNotFoundException($"Teacher with ID {teacherId} does not exist.");

            var student = await _repository.GetByIdAsync(studentId);
            if (student is null)
                throw new KeyNotFoundException($"Student with ID {studentId} does not exist.");

            await _repository.AssignToTeacherAsync(studentId, teacherId);
            _logger.LogInformation("Teacher {TeacherId} successfully assigned to student {StudentId}", teacherId, studentId);
        }

        /// <inheritdoc />
        public async Task UnassignStudentFromTeacherAsync(int studentId, int teacherId)
        {
            _logger.LogInformation("Unassigning teacher {TeacherId} from student {StudentId}", teacherId, studentId);

            if (!await _repository.IsAssignedToTeacherAsync(studentId, teacherId))
                throw new KeyNotFoundException($"Student with ID {studentId} is not assigned to you.");

            // Prevent leaving the student with zero teachers (orphaned — invisible to all teachers).
            var assignmentCount = await _repository.CountTeacherAssignmentsAsync(studentId);
            if (assignmentCount <= 1)
                throw new InvalidOperationException(
                    $"Cannot unassign: student {studentId} would have no teachers remaining. " +
                    "Assign another teacher first, or delete the student.");

            await _repository.UnassignFromTeacherAsync(studentId, teacherId);
            _logger.LogInformation("Teacher {TeacherId} successfully unassigned from student {StudentId}", teacherId, studentId);
        }
    }
}
