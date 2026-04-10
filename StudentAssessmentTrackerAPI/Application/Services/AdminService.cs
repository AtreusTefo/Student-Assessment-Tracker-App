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

        /// <summary>Deletes a teacher account regardless of student assignments.</summary>
        Task DeleteTeacherAsync(int teacherId);

        /// <summary>Returns all students across all teachers.</summary>
        Task<IEnumerable<StudentDto>> GetAllStudentsAsync();

        /// <summary>Deletes a student account regardless of which teacher owns them.</summary>
        Task DeleteStudentAsync(int studentId);
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
        public async Task DeleteStudentAsync(int studentId)
        {
            var student = await _db.Students.FindAsync(studentId)
                ?? throw new KeyNotFoundException($"Student {studentId} not found.");
            _db.Students.Remove(student);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Admin deleted student {StudentId}", studentId);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

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
    }
}
