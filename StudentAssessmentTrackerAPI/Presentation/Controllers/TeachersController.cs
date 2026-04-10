using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Application.Services;

namespace StudentAssessmentTracker.Presentation.Controllers
{
    /// <summary>
    /// REST API controller for teacher operations
    /// Follows Clean Architecture — uses only Application layer abstractions
    /// </summary>
    [ApiController]
    [Route("api/teachers")]
    [Produces("application/json")]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly IAuditLogService _auditLog;
        private readonly ILogger<TeachersController> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="TeachersController"/>
        /// </summary>
        public TeachersController(
            ITeacherService teacherService,
            IAuditLogService auditLog,
            ILogger<TeachersController> logger)
        {
            _teacherService = teacherService;
            _auditLog = auditLog;
            _logger = logger;
        }

        // ====================================================================
        // GET /api/teachers
        // ====================================================================

        /// <summary>
        /// Retrieves all teachers
        /// </summary>
        /// <returns>List of all teachers</returns>
        /// <response code="200">Teachers retrieved successfully</response>
        /// <response code="401">Not authenticated</response>
        /// <response code="403">Not authorized (Teacher role required)</response>
        [Authorize(Roles = "Teacher")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TeacherResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("GET /api/teachers");
            var teachers = await _teacherService.GetAllTeachersAsync();
            // Issue 6 fix: an empty collection is a valid 200 OK — 404 is semantically wrong
            // for a collection endpoint and breaks Angular error handling that treats 404 as
            // a hard failure rather than an empty state.
            return Ok(teachers ?? Enumerable.Empty<TeacherResponseDto>());
        }

        // ====================================================================
        // GET /api/teachers/{id}
        // ====================================================================

        /// <summary>
        /// Retrieves a single teacher by ID
        /// </summary>
        /// <param name="id">The teacher identifier</param>
        /// <returns>Teacher data</returns>
        /// <response code="200">Teacher found</response>
        /// <response code="401">Not authenticated</response>
        /// <response code="403">Not authorized (Teacher role required)</response>
        /// <response code="404">Teacher not found</response>
        [Authorize(Roles = "Teacher")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TeacherResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("GET /api/teachers/{Id}", id);
            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            return teacher is null ? NotFound(new { message = $"Teacher with ID {id} not found." }) : Ok(teacher);
        }

        // ====================================================================
        // POST /api/teachers
        // ====================================================================

        /// <summary>
        /// Creates (registers) a new teacher
        /// </summary>
        /// <param name="dto">Teacher registration data</param>
        /// <returns>Created teacher</returns>
        /// <response code="201">Teacher created successfully</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        [ProducesResponseType(typeof(TeacherResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] TeacherRegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("POST /api/teachers — registering {Email}", dto.Email);
            try
            {
                var created = await _teacherService.CreateTeacherAsync(dto);
                _ = _auditLog.LogAsync("Teacher", created.TeacherId, "Create", null,
                    JsonSerializer.Serialize(new { created.Email, created.SubjectName }),
                    created.TeacherId.ToString(), "Teacher");
                return CreatedAtAction(nameof(GetById), new { id = created.TeacherId }, created);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating teacher {Email}", dto.Email);
                return StatusCode(500, new { message = "Internal server error while creating teacher." });
            }
        }

        // ====================================================================
        // PUT /api/teachers/{id}
        // ====================================================================

        /// <summary>
        /// Updates the authenticated teacher's own profile.
        /// Teachers may only update their own record — attempting to update another teacher's record returns 403.
        /// </summary>
        /// <param name="id">The teacher identifier (must match the authenticated teacher's ID)</param>
        /// <param name="dto">Updated teacher data</param>
        /// <returns>Confirmation message on success</returns>
        /// <response code="200">Teacher updated successfully</response>
        /// <response code="401">Not authenticated</response>
        /// <response code="403">Authenticated teacher does not own this record</response>
        /// <response code="404">Teacher not found</response>
        [Authorize]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] TeacherUpdateDto dto)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            if (id != teacherId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You may only update your own profile." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("PUT /api/teachers/{Id} by authenticated teacher {TeacherId}", id, teacherId);
            try
            {
                var updated = await _teacherService.UpdateTeacherAsync(id, dto);
                if (updated)
                    _ = _auditLog.LogAsync("Teacher", id, "Update", null,
                        JsonSerializer.Serialize(new { dto.Email, dto.Phone }),
                        teacherId.ToString(), "Teacher");
                return updated ? Ok(new { message = $"Teacher with ID {id} successfully updated." }) : NotFound(new { message = $"Teacher with ID {id} not found." });
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating teacher {TeacherId}", id);
                return StatusCode(500, new { message = "Internal server error while updating teacher." });
            }
        }

        // ====================================================================
        // DELETE /api/teachers/{id}
        // ====================================================================

        /// <summary>
        /// Deletes the authenticated teacher's own account.
        /// Teachers may only delete their own record — attempting to delete another teacher's record returns 403.
        /// </summary>
        /// <param name="id">The teacher identifier (must match the authenticated teacher's ID)</param>
        /// <returns>Confirmation message on success</returns>
        /// <response code="200">Teacher deleted successfully</response>
        /// <response code="401">Not authenticated</response>
        /// <response code="403">Authenticated teacher does not own this record</response>
        /// <response code="404">Teacher not found</response>
        /// <response code="409">Teacher still has students assigned</response>
        [Authorize]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            if (id != teacherId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You may only delete your own account." });

            _logger.LogInformation("DELETE /api/teachers/{Id} by authenticated teacher {TeacherId}", id, teacherId);
            try
            {
                var deleted = await _teacherService.DeleteTeacherAsync(id);
                if (deleted)
                    _ = _auditLog.LogAsync("Teacher", id, "Delete",
                        $"{{\"id\":{id}}}", null, teacherId.ToString(), "Teacher");
                return deleted
                    ? Ok(new { message = $"Teacher with ID {id} successfully deleted." })
                    : NotFound(new { message = $"Teacher with ID {id} not found." });
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting teacher {TeacherId}", id);
                return StatusCode(500, new { message = "Internal server error while deleting teacher." });
            }
        }

        // ====================================================================
        // POST /api/teachers/login
        // ====================================================================

        /// <summary>
        /// Authenticates a teacher with email and password
        /// </summary>
        /// <param name="dto">Login credentials</param>
        /// <returns>Token and teacher profile on success</returns>
        /// <response code="200">Login successful</response>
        /// <response code="401">Invalid credentials</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(TeacherLoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] TeacherLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("POST /api/teachers/login for {Email}", dto.Email);
            var result = await _teacherService.LoginAsync(dto);
            return result is null
                ? Unauthorized(new { message = "Invalid email or password." })
                : Ok(result);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Extracts the teacherId claim from the validated JWT. Returns false when missing or non-numeric.</summary>
        private bool TryGetTeacherId(out int teacherId)
        {
            teacherId = 0;
            var value = User.FindFirstValue("teacherId");
            return value != null && int.TryParse(value, out teacherId);
        }
    }
}
