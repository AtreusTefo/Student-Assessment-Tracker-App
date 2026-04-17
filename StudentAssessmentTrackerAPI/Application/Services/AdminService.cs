using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Infrastructure.Data;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>Contract for admin account management and authentication.</summary>
    public interface IAdminService
    {
        /// <summary>Creates a new admin account.</summary>
        Task<AdminDto> CreateAdminAsync(CreateAdminDto dto);

        /// <summary>Returns the admin with <paramref name="id"/>, or null if not found.</summary>
        Task<AdminDto?> GetByIdAsync(int id);

        /// <summary>Authenticates an admin and returns a signed JWT on success.</summary>
        Task<AdminLoginResponseDto?> LoginAsync(AdminLoginDto dto);

        /// <summary>Returns all registered teachers (admin view — no teacher-scope filter).</summary>
        Task<IEnumerable<TeacherResponseDto>> GetAllTeachersAsync();

        /// <summary>
        /// Creates a new teacher account (admin-only). Password is BCrypt-hashed before storage.
        /// </summary>
        Task<TeacherResponseDto> CreateTeacherAsync(TeacherRegisterDto dto);

        /// <summary>Deletes a teacher account regardless of student assignments.</summary>
        Task DeleteTeacherAsync(int teacherId);

        /// <summary>Returns all students across all teachers.</summary>
        Task<IEnumerable<StudentDto>> GetAllStudentsAsync();

        /// <summary>Returns the student with <paramref name="studentId"/>, or null if not found.</summary>
        Task<StudentDto?> GetStudentByIdAsync(int studentId);

        /// <summary>
        /// Creates a new student account (school enrollment). No teacher assignment is made —
        /// timetabling is a separate admin operation via AssignStudentToTeacherAsync.
        /// </summary>
        Task<StudentDto> CreateStudentAsync(CreateStudentDto dto);

        /// <summary>
        /// Updates a student's profile data (name, grade, email, phone, ID/Passport).
        /// Admin-scoped — not restricted to a particular teacher's roster.
        /// </summary>
        Task<StudentDto> UpdateStudentAsync(int studentId, UpdateStudentDto dto);

        /// <summary>Deletes a student account regardless of which teacher owns them.</summary>
        Task DeleteStudentAsync(int studentId);

        /// <summary>
        /// Assigns <paramref name="teacherId"/> to <paramref name="studentId"/> (timetabling).
        /// Enforces subject uniqueness: one teacher per subject per student.
        /// </summary>
        Task AssignStudentToTeacherAsync(int studentId, int teacherId);

        /// <summary>
        /// Removes the assignment between <paramref name="teacherId"/> and <paramref name="studentId"/>.
        /// Throws <see cref="KeyNotFoundException"/> when the assignment does not exist.
        /// </summary>
        Task UnassignStudentFromTeacherAsync(int studentId, int teacherId);

        /// <summary>
        /// Changes the password for admin with <paramref name="adminId"/>.
        /// Requires the current password for verification.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Admin not found.</exception>
        /// <exception cref="UnauthorizedAccessException">Current password is incorrect.</exception>
        /// <exception cref="ArgumentException">New passwords do not match or fail length requirement.</exception>
        Task ChangePasswordAsync(int adminId, ChangeAdminPasswordDto dto);
    }

    /// <summary>Implements admin account management and cross-entity oversight operations.</summary>
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>Initialises the service.</summary>
        public AdminService(
            ApplicationDbContext db,
            IMapper mapper,
            ILogger<AdminService> logger,
            IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
        }

        /// <inheritdoc />
        public async Task<AdminDto> CreateAdminAsync(CreateAdminDto dto)
        {
            if (await _db.Admins.AnyAsync(a => a.Email == dto.Email!.ToLower()))
                throw new InvalidOperationException($"An admin with email '{dto.Email}' already exists.");

            // Issue #6: cross-entity email uniqueness — email must be unique across Admins,
            // Students, and Teachers to prevent credential confusion and impersonation.
            var emailLower = dto.Email!.Trim().ToLower();
            if (await _db.Students.AnyAsync(s => s.Email == emailLower))
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use by a student account.");
            if (await _db.Teachers.AnyAsync(t => t.Email == emailLower))
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use by a teacher account.");

            var admin = new Admin
            {
                FirstName = dto.FirstName!.Trim(),
                LastName = dto.LastName!.Trim(),
                Email = dto.Email!.Trim().ToLower(),
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Admins.Add(admin);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Admin account created for {Email}", admin.Email);
            return MapToDto(admin);
        }

        /// <inheritdoc />
        public async Task<AdminDto?> GetByIdAsync(int id)
        {
            var admin = await _db.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            return admin is null ? null : MapToDto(admin);
        }

        /// <inheritdoc />
        public async Task<AdminLoginResponseDto?> LoginAsync(AdminLoginDto dto)
        {
            var email = dto.Email?.Trim().ToLower();
            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Email == email);
            if (admin is null || !BCrypt.Net.BCrypt.Verify(dto.Password, admin.Password))
            {
                _logger.LogWarning("Failed admin login attempt for {Email}", email);
                return null;
            }

            var token = GenerateJwt(admin);
            return new AdminLoginResponseDto { Token = token, Admin = MapToDto(admin) };
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TeacherResponseDto>> GetAllTeachersAsync()
        {
            var teachers = await _db.Teachers
                .AsNoTracking()
                .Include(t => t.SubjectNavigation)
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ToListAsync();
            return _mapper.Map<IEnumerable<TeacherResponseDto>>(teachers);
        }

        /// <inheritdoc />
        public async Task<TeacherResponseDto> CreateTeacherAsync(TeacherRegisterDto dto)
        {
            // Validate SubjectId against lookup table.
            if (!await _db.Subjects.AnyAsync(s => s.Id == dto.SubjectId))
                throw new ArgumentException($"Subject with ID {dto.SubjectId} does not exist.");

            // Cross-entity email uniqueness.
            var emailLower = dto.Email.Trim().ToLower();
            if (await _db.Teachers.AnyAsync(t => t.Email == emailLower))
                throw new InvalidOperationException($"A teacher with email '{dto.Email}' is already registered.");
            if (await _db.Students.AnyAsync(s => s.Email == emailLower))
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use by a student account.");
            if (await _db.Admins.AnyAsync(a => a.Email == emailLower))
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use by an admin account.");

            // ID/Passport uniqueness.
            var idNormalized = dto.IdPassportNo.ToUpperInvariant();
            if (await _db.Teachers.AnyAsync(t => t.IdPassportNo != null && t.IdPassportNo.ToUpper() == idNormalized))
                throw new InvalidOperationException($"A teacher with ID/Passport No. '{dto.IdPassportNo}' is already registered.");

            var teacher = new Teacher
            {
                IdPassportNo = dto.IdPassportNo.Trim(),
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = emailLower,
                Phone = dto.Phone.Trim(),
                SubjectId = dto.SubjectId,
                // Password is intentionally NOT set — teacher activates their own account
                // via POST /api/teachers/activate using their email address.
                EnrollmentDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Teachers.Add(teacher);
            await _db.SaveChangesAsync();

            // Reload with navigation property for SubjectName in the response DTO.
            var created = await _db.Teachers
                .Include(t => t.SubjectNavigation)
                .FirstAsync(t => t.Id == teacher.Id);

            _logger.LogInformation("Admin created teacher {TeacherId} {Email}", teacher.Id, teacher.Email);
            return _mapper.Map<TeacherResponseDto>(created);
        }

        /// <inheritdoc />
        public async Task DeleteTeacherAsync(int teacherId)
        {
            var teacher = await _db.Teachers.FindAsync(teacherId)
                ?? throw new KeyNotFoundException($"Teacher {teacherId} not found.");
            _db.Teachers.Remove(teacher);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Admin deleted teacher {TeacherId}", teacherId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
        {
            var students = await _db.Students
                .AsNoTracking()
                .Include(s => s.GradeNavigation)
                .Include(s => s.Assessments)
                    .ThenInclude(a => a.Submissions)
                .Include(s => s.TeacherAssignments)
                    .ThenInclude(ta => ta.Teacher)
                .Include(s => s.TeacherAssignments)
                    .ThenInclude(ta => ta.Subject)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();
            return _mapper.Map<IEnumerable<StudentDto>>(students);
        }

        /// <inheritdoc />
        public async Task<StudentDto?> GetStudentByIdAsync(int studentId)
        {
            var student = await _db.Students
                .AsNoTracking()
                .Include(s => s.GradeNavigation)
                .Include(s => s.Assessments).ThenInclude(a => a.Submissions)
                .Include(s => s.TeacherAssignments).ThenInclude(ta => ta.Teacher)
                .Include(s => s.TeacherAssignments).ThenInclude(ta => ta.Subject)
                .FirstOrDefaultAsync(s => s.Id == studentId);
            return student is null ? null : _mapper.Map<StudentDto>(student);
        }

        /// <inheritdoc />
        public async Task DeleteStudentAsync(int studentId)
        {
            var student = await _db.Students.FindAsync(studentId)
                ?? throw new KeyNotFoundException($"Student {studentId} not found.");
            _db.Students.Remove(student);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Admin deleted student {StudentId}", studentId);
        }

        /// <inheritdoc />
        public async Task<StudentDto> CreateStudentAsync(CreateStudentDto dto)
        {
            // Validate grade lookup.
            if (!await _db.Grades.AnyAsync(g => g.Id == dto.GradeId))
                throw new ArgumentException($"Grade with ID {dto.GradeId} does not exist.");

            // Cross-entity email uniqueness.
            var emailLower = dto.Email!.Trim().ToLower();
            if (await _db.Students.AnyAsync(s => s.Email == emailLower))
                throw new InvalidOperationException($"A student with email '{dto.Email}' is already registered.");
            if (await _db.Teachers.AnyAsync(t => t.Email == emailLower))
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use by a teacher account.");
            if (await _db.Admins.AnyAsync(a => a.Email == emailLower))
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use by an admin account.");

            // ID/Passport uniqueness.
            var idNormalized = dto.IdPassportNo!.ToUpperInvariant();
            if (await _db.Students.AnyAsync(s => s.IdPassportNo != null && s.IdPassportNo.ToUpper() == idNormalized))
                throw new InvalidOperationException($"A student with ID/Passport No. '{dto.IdPassportNo}' is already registered.");

            var student = new Student
            {
                IdPassportNo = dto.IdPassportNo!.Trim(),
                FirstName = dto.FirstName!.Trim(),
                LastName = dto.LastName!.Trim(),
                Email = emailLower,
                Phone = dto.Phone!.Trim(),
                GradeId = dto.GradeId,
                // No password — student activates their own account via POST /api/students/activate.
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Generate unique STU-XXXXXXXX identifier with collision retry.
            const int maxAttempts = 5;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                student.StudentUniqueId = GenerateStudentUniqueId();
                if (!await _db.Students.AnyAsync(s => s.StudentUniqueId == student.StudentUniqueId))
                    break;
            }

            _db.Students.Add(student);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new InvalidOperationException("A student with that email or ID/Passport number already exists.");
            }

            // Reload with navigation properties for a complete DTO response.
            var created = await _db.Students
                .Include(s => s.GradeNavigation)
                .Include(s => s.Assessments).ThenInclude(a => a.Submissions)
                .Include(s => s.TeacherAssignments).ThenInclude(ta => ta.Teacher)
                .Include(s => s.TeacherAssignments).ThenInclude(ta => ta.Subject)
                .FirstAsync(s => s.Id == student.Id);

            _logger.LogInformation("Admin created student {StudentId} {UniqueId}", student.Id, student.StudentUniqueId);
            return _mapper.Map<StudentDto>(created);
        }

        /// <inheritdoc />
        public async Task<StudentDto> UpdateStudentAsync(int studentId, UpdateStudentDto dto)
        {
            var student = await _db.Students
                .Include(s => s.ClassGroupEnrollments).ThenInclude(cgs => cgs.ClassGroup)
                .FirstOrDefaultAsync(s => s.Id == studentId)
                ?? throw new KeyNotFoundException($"Student {studentId} not found.");

            // Validate grade lookup.
            if (!await _db.Grades.AnyAsync(g => g.Id == dto.GradeId))
                throw new ArgumentException($"Grade with ID {dto.GradeId} does not exist.");

            // Block grade change while enrolled in class groups at the current grade.
            if (dto.GradeId != student.GradeId)
            {
                var gradeConflict = student.ClassGroupEnrollments
                    .Any(cgs => cgs.ClassGroup != null && cgs.ClassGroup.GradeId != dto.GradeId);
                if (gradeConflict)
                    throw new InvalidOperationException(
                        $"Cannot change grade: student {studentId} is enrolled in class groups at their " +
                        "current grade level. Unenroll from all class groups before changing grade.");
            }

            // Cross-entity email uniqueness (excluding self).
            var emailLower = dto.Email!.Trim().ToLower();
            if (await _db.Students.AnyAsync(s => s.Email == emailLower && s.Id != studentId))
                throw new InvalidOperationException($"A student with email '{dto.Email}' is already registered.");
            if (await _db.Teachers.AnyAsync(t => t.Email == emailLower))
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use by a teacher account.");
            if (await _db.Admins.AnyAsync(a => a.Email == emailLower))
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use by an admin account.");

            // ID/Passport uniqueness (excluding self).
            var idNormalized = dto.IdPassportNo!.ToUpperInvariant();
            if (await _db.Students.AnyAsync(s => s.IdPassportNo != null && s.IdPassportNo.ToUpper() == idNormalized && s.Id != studentId))
                throw new InvalidOperationException($"A student with ID/Passport No. '{dto.IdPassportNo}' is already registered.");

            student.IdPassportNo = dto.IdPassportNo!.Trim();
            student.FirstName = dto.FirstName!.Trim();
            student.LastName = dto.LastName!.Trim();
            student.Email = emailLower;
            student.Phone = dto.Phone!.Trim();
            student.GradeId = dto.GradeId;
            student.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            _logger.LogInformation("Admin updated student {StudentId}", studentId);

            // Reload with full navigation properties for the response DTO.
            var updated = await _db.Students
                .Include(s => s.GradeNavigation)
                .Include(s => s.Assessments).ThenInclude(a => a.Submissions)
                .Include(s => s.TeacherAssignments).ThenInclude(ta => ta.Teacher)
                .Include(s => s.TeacherAssignments).ThenInclude(ta => ta.Subject)
                .FirstAsync(s => s.Id == studentId);
            return _mapper.Map<StudentDto>(updated);
        }

        /// <inheritdoc />
        public async Task AssignStudentToTeacherAsync(int studentId, int teacherId)
        {
            var student = await _db.Students.FindAsync(studentId)
                ?? throw new KeyNotFoundException($"Student {studentId} not found.");
            var teacher = await _db.Teachers.FindAsync(teacherId)
                ?? throw new KeyNotFoundException($"Teacher {teacherId} not found.");

            // Referential integrity: check for existing assignment (idempotent) or subject conflict.
            var existing = await _db.TeacherStudents
                .AsNoTracking()
                .FirstOrDefaultAsync(ts => ts.StudentId == studentId
                    && (ts.TeacherId == teacherId || ts.SubjectId == teacher.SubjectId));

            if (existing != null && existing.TeacherId == teacherId) return; // already assigned — idempotent

            if (existing != null && existing.SubjectId == teacher.SubjectId)
                throw new InvalidOperationException(
                    $"Student {studentId} already has a teacher assigned for subject {teacher.SubjectId}. " +
                    "Unassign the existing teacher for that subject first.");

            try
            {
                _db.TeacherStudents.Add(new TeacherStudent
                {
                    TeacherId = teacherId,
                    StudentId = studentId,
                    SubjectId = teacher.SubjectId,
                    AssignedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Concurrent request inserted the same row first — desired state achieved.
            }

            _logger.LogInformation("Admin assigned teacher {TeacherId} to student {StudentId}", teacherId, studentId);
        }

        /// <inheritdoc />
        public async Task UnassignStudentFromTeacherAsync(int studentId, int teacherId)
        {
            var assignment = await _db.TeacherStudents
                .FirstOrDefaultAsync(ts => ts.TeacherId == teacherId && ts.StudentId == studentId)
                ?? throw new KeyNotFoundException($"Teacher {teacherId} is not assigned to student {studentId}.");

            _db.TeacherStudents.Remove(assignment);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Admin unassigned teacher {TeacherId} from student {StudentId}", teacherId, studentId);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <inheritdoc />
        public async Task ChangePasswordAsync(int adminId, ChangeAdminPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                throw new ArgumentException("New password must be at least 6 characters.");

            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new ArgumentException("New password and confirmation do not match.");

            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Id == adminId)
                ?? throw new KeyNotFoundException($"Admin {adminId} not found.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, admin.Password))
                throw new UnauthorizedAccessException("Current password is incorrect.");

            admin.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            admin.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Password changed for admin {AdminId}", adminId);
        }

        private string GenerateJwt(Admin admin)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, admin.Email),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static AdminDto MapToDto(Admin a) => new()
        {
            Id = a.Id,
            FirstName = a.FirstName,
            LastName = a.LastName,
            Email = a.Email,
            CreatedAt = a.CreatedAt
        };

        private static string GenerateStudentUniqueId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var suffix = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
            return $"STU-{suffix}";
        }

        private static bool IsUniqueConstraintViolation(Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            var inner = ex.InnerException;
            while (inner is not null)
            {
                if (inner is Microsoft.Data.SqlClient.SqlException sqlEx &&
                    (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                    return true;
                inner = inner.InnerException;
            }
            return false;
        }
    }
}
