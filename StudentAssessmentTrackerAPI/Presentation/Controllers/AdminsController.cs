using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Application.Services;

namespace StudentAssessmentTracker.Presentation.Controllers
{
    /// <summary>
    /// Admin management endpoints. All routes except POST /api/admins/login require
    /// a JWT issued with the "Admin" role claim.
    /// </summary>
    [ApiController]
    [Route("api/admins")]
    [Produces("application/json")]
    public class AdminsController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IAuditLogService _auditLog;
        private readonly ILogger<AdminsController> _logger;

        /// <summary>Initialises the controller.</summary>
        public AdminsController(
            IAdminService adminService,
            IAuditLogService auditLog,
            ILogger<AdminsController> logger)
        {
            _adminService = adminService;
            _auditLog = auditLog;
            _logger = logger;
        }

        // ── Authentication ────────────────────────────────────────────────────

        /// <summary>Authenticates an admin and returns a signed JWT.</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AdminLoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
        {
            var result = await _adminService.LoginAsync(dto);
            return result is null
                ? Unauthorized(new { message = "Invalid email or password." })
                : Ok(result);
        }

        /// <summary>
        /// Registers a new admin account. This endpoint itself requires an existing Admin JWT
        /// to prevent open self-registration (an existing admin must create new admins).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(AdminDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateAdminDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var admin = await _adminService.CreateAdminAsync(dto);
                await _auditLog.LogAsync("Admin", admin.Id, "Create", null,
                    $"{{\"email\":\"{admin.Email}\"}}", GetCallerId(), "Admin");
                return CreatedAtAction(nameof(GetById), new { id = admin.Id }, admin);
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        /// <summary>Returns the admin profile for the given ID.</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AdminDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var admin = await _adminService.GetByIdAsync(id);
            return admin is null ? NotFound(new { message = $"Admin {id} not found." }) : Ok(admin);
        }

        /// <summary>
        /// Changes the password for an admin account.
        /// An admin may only change their own password (id must match the JWT claim).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangeAdminPasswordDto dto)
        {
            var callerId = GetCallerId();
            if (callerId is null || callerId != id.ToString())
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You may only change your own password." });

            try
            {
                await _adminService.ChangePasswordAsync(id, dto);
                await _auditLog.LogAsync("Admin", id, "Update",
                    null, "{\"passwordChanged\":true}", callerId, "Admin");
                return Ok(new { message = "Password changed successfully." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ── Teacher oversight ────────────────────────────────────────────────

        /// <summary>Returns all registered teachers (admin view — no scope restriction).</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("teachers")]
        [ProducesResponseType(typeof(IEnumerable<TeacherResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeachers()
        {
            var teachers = await _adminService.GetAllTeachersAsync();
            return Ok(teachers);
        }

        /// <summary>Deletes a teacher account and all related records (admin override).</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("teachers/{teacherId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTeacher(int teacherId)
        {
            try
            {
                await _adminService.DeleteTeacherAsync(teacherId);
                await _auditLog.LogAsync("Teacher", teacherId, "Delete",
                    $"{{\"id\":{teacherId}}}", null, GetCallerId(), "Admin");
                return Ok(new { message = $"Teacher {teacherId} deleted by admin." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ── Student oversight ────────────────────────────────────────────────

        /// <summary>Returns all students across all teachers (admin view).</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("students")]
        [ProducesResponseType(typeof(IEnumerable<StudentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _adminService.GetAllStudentsAsync();
            return Ok(students);
        }

        /// <summary>Deletes a student account regardless of teacher assignment (admin override).</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("students/{studentId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteStudent(int studentId)
        {
            try
            {
                await _adminService.DeleteStudentAsync(studentId);
                await _auditLog.LogAsync("Student", studentId, "Delete",
                    $"{{\"id\":{studentId}}}", null, GetCallerId(), "Admin");
                return Ok(new { message = $"Student {studentId} deleted by admin." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ── Audit Log ────────────────────────────────────────────────────────

        /// <summary>Returns all audit log entries, newest first (paginated).</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("audit-logs")]
        [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            pageSize = Math.Min(pageSize, 200);
            var logs = await _auditLog.GetAllAsync(page, pageSize);
            return Ok(logs);
        }

        /// <summary>Returns audit log entries for a specific entity type and primary key.</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("audit-logs/{entityName}/{entityId:int}")]
        [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogsByEntity(string entityName, int entityId)
        {
            var logs = await _auditLog.GetByEntityAsync(entityName, entityId);
            return Ok(logs);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private string? GetCallerId() =>
            User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
    }
}
