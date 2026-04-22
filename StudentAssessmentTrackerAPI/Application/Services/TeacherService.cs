using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;
using StudentAssessmentTracker.Infrastructure.Data;

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
        /// Throws <see cref="InvalidOperationException"/> when the account has not been activated.
        /// </summary>
        Task<TeacherLoginResponseDto?> LoginAsync(TeacherLoginDto dto);

        /// <summary>
        /// Activates a teacher account by setting the initial password.
        /// Returns <c>null</c> when no teacher with that email exists.
        /// Throws <see cref="InvalidOperationException"/> when the account is already active.
        /// </summary>
        Task<TeacherLoginResponseDto?> ActivateTeacherAsync(TeacherActivateDto dto);

        /// <summary>
        /// Resets a teacher's password by nulling it so they must re-activate.
        /// Throws <see cref="KeyNotFoundException"/> when no teacher with that email exists.
        /// </summary>
        Task ForgotPasswordAsync(TeacherForgotPasswordDto dto);
    }

    /// <summary>
    /// Handles teacher CRUD, duplicate-detection, FK-safe deletion, and JWT-based authentication.
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _repository;
        private readonly IRepository<Subject> _subjectRepository;
        private readonly ApplicationDbContext _db;
        private readonly IAuditLogService _auditLog;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>Initialises the service with the teacher repository, subject repository, mapper, logger, and configuration.</summary>
        public TeacherService(
            ITeacherRepository repository,
            IRepository<Subject> subjectRepository,
            ApplicationDbContext db,
            IAuditLogService auditLog,
            IMapper mapper,
            ILogger<TeacherService> logger,
            IConfiguration configuration)
        {
            _repository = repository;
            _subjectRepository = subjectRepository;
            _db = db;
            _auditLog = auditLog;
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

            // Issue #6: cross-entity email uniqueness — email must be unique across Teachers,
            // Students, and Admins to prevent credential confusion and impersonation.
            var emailLower = dto.Email.Trim().ToLower();
            if (await _db.Students.AnyAsync(s => s.Email == emailLower))
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use by a student account.");
            if (await _db.Admins.AnyAsync(a => a.Email == emailLower))
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use by an admin account.");

            // Issue 4: detect duplicate ID/Passport number.
            if (await _repository.ExistsByIdPassportNoAsync(dto.IdPassportNo))
                throw new InvalidOperationException($"A teacher with ID/Passport No. '{dto.IdPassportNo}' is already registered.");

            var teacher = _mapper.Map<Teacher>(dto);
            // SECURITY: password is NOT set here — teacher activates their own account via POST /api/teachers/activate.
            // Issue #4: normalise email to lowercase before storage.
            teacher.Email = dto.Email.Trim().ToLower();
            teacher.CreatedDate = DateTime.UtcNow;
            // Issue 5: AddAsync already calls SaveChangesAsync internally — do not call it again.
            await _repository.AddAsync(teacher);

            await _auditLog.LogAsync("Teacher", teacher.Id, "Create",
                oldValues: null,
                newValues: JsonSerializer.Serialize(new { teacher.Email, teacher.SubjectId, teacher.FirstName, teacher.LastName }),
                changedBy: teacher.Email, changedByRole: "Teacher");

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

            // Issue #3: cross-entity email uniqueness — UpdateTeacherAsync must repeat the
            // same cross-table checks that CreateTeacherAsync applies. Omitting this would
            // allow a teacher to change their email to one already held by a student or admin.
            var updateEmailLower = dto.Email.Trim().ToLower();
            if (await _db.Students.AnyAsync(s => s.Email == updateEmailLower))
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use by a student account.");
            if (await _db.Admins.AnyAsync(a => a.Email == updateEmailLower))
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use by an admin account.");

            // Detect duplicate ID/Passport number, excluding the record being updated.
            if (await _repository.ExistsByIdPassportNoAsync(dto.IdPassportNo, excludeTeacherId: id))
                throw new InvalidOperationException($"A teacher with ID/Passport No. '{dto.IdPassportNo}' is already registered.");

            _mapper.Map(dto, teacher);
            // Issue #4: normalise email to lowercase before storage on update.
            teacher.Email = dto.Email.Trim().ToLower();
            teacher.UpdatedAt = DateTime.UtcNow; // Issue #8: stamp UpdatedAt on every update
            await _repository.UpdateAsync(teacher);

            await _auditLog.LogAsync("Teacher", id, "Update",
                oldValues: null,
                newValues: JsonSerializer.Serialize(new { teacher.Email, teacher.SubjectId, teacher.FirstName, teacher.LastName }),
                changedBy: teacher.Email, changedByRole: "Teacher");

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

            // Issue #2: also block deletion when the teacher owns class groups that still
            // have enrolled students. The DB cascade would silently remove those class groups
            // AND all enrollments — data loss with no audit trail.
            if (await _db.ClassGroups.AnyAsync(cg => cg.TeacherId == id &&
                    cg.Enrollments.Any()))
                throw new InvalidOperationException(
                    $"Teacher with ID {id} cannot be deleted because they have class groups with enrolled students. " +
                    "Unenroll all students from their class groups first.");

            // DeleteAsync already calls SaveChangesAsync internally — no second call needed.
            await _repository.DeleteAsync(id);

            await _auditLog.LogAsync("Teacher", id, "Delete",
                oldValues: JsonSerializer.Serialize(new { teacher.Email, teacher.SubjectId }),
                newValues: null,
                changedBy: teacher.Email, changedByRole: "Teacher");

            return true;
        }

        /// <inheritdoc />
        public async Task<TeacherLoginResponseDto?> LoginAsync(TeacherLoginDto dto)
        {
            _logger.LogInformation("Login attempt for email {Email}", dto.Email);

            var teacher = await _repository.FindByEmailAsync(dto.Email);

            if (teacher is null)
            {
                _logger.LogWarning("Failed login attempt for email {Email}", dto.Email);
                return null;
            }

            if (string.IsNullOrEmpty(teacher.Password))
                throw new InvalidOperationException("Account not yet activated. Please use POST /api/teachers/activate to set your password.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, teacher.Password))
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

        /// <inheritdoc />
        public async Task<TeacherLoginResponseDto?> ActivateTeacherAsync(TeacherActivateDto dto)
        {
            _logger.LogInformation("Activation attempt for teacher email {Email}", dto.Email);

            var emailLower = dto.Email.Trim().ToLower();
            var teacher = await _repository.FindByEmailAsync(emailLower);
            if (teacher is null)
            {
                _logger.LogWarning("Activation failed - no teacher found for email {Email}", emailLower);
                return null;
            }

            if (!string.IsNullOrEmpty(teacher.Password))
                throw new InvalidOperationException("This account has already been activated. Please use the login page.");

            if (dto.Password != dto.ConfirmPassword)
                throw new ArgumentException("Password and confirmation password do not match.");

            // OWASP A02: BCrypt - same approach as student and admin activation.
            teacher.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            teacher.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(teacher);

            await _auditLog.LogAsync("Teacher", teacher.Id, "Update",
                oldValues: JsonSerializer.Serialize(new { Activated = false }),
                newValues: JsonSerializer.Serialize(new { Activated = true }),
                changedBy: teacher.Email, changedByRole: "Teacher");

            _logger.LogInformation("Teacher {TeacherId} account activated", teacher.Id);
            return new TeacherLoginResponseDto
            {
                Token = GenerateJwtToken(teacher),
                Teacher = _mapper.Map<TeacherResponseDto>(teacher)
            };
        }

        /// <inheritdoc />
        public async Task ForgotPasswordAsync(TeacherForgotPasswordDto dto)
        {
            var emailLower = dto.Email.Trim().ToLower();
            var teacher = await _repository.FindByEmailAsync(emailLower);
            if (teacher is null)
                throw new KeyNotFoundException($"No teacher account found with email '{dto.Email}'.");

            teacher.Password = null;
            teacher.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(teacher);

            await _auditLog.LogAsync("Teacher", teacher.Id, "ForgotPassword",
                oldValues: JsonSerializer.Serialize(new { PasswordCleared = true }),
                newValues: null,
                changedBy: teacher.Email, changedByRole: "Teacher");

            _logger.LogInformation("Teacher {TeacherId} password reset via forgot-password", teacher.Id);
        }

        // ── Private helpers ───────────────────────────────────────────────────

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
