using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Application.Services;

namespace StudentAssessmentTracker.Presentation.Controllers
{
    /// <summary>
    /// REST API controller for student operations.
    /// Teacher-facing CRUD endpoints require a valid JWT; student self-service endpoints
    /// (activate, login) are publicly accessible.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IAuditLogService _auditLog;
        private readonly ILogger<StudentsController> _logger;

        /// <summary>Initialises the controller with the student service and logger.</summary>
        public StudentsController(
            IStudentService studentService,
            IAuditLogService auditLog,
            ILogger<StudentsController> logger)
        {
            _studentService = studentService;
            _auditLog = auditLog;
            _logger = logger;
        }

        // ── Teacher-authenticated endpoints ───────────────────────────────────

        /// <summary>Returns all students belonging to the authenticated teacher.</summary>
        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetAllStudents()
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                _logger.LogInformation("GetAllStudents for teacher {TeacherId}", teacherId);
                var students = await _studentService.GetAllStudentsAsync(teacherId);
                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching students for teacher {TeacherId}", teacherId);
                return StatusCode(500, new { message = "Internal server error while fetching students" });
            }
        }

        /// <summary>Returns a single student by ID, scoped to the authenticated teacher.</summary>
        /// <param name="id">The student's primary key.</param>
        [Authorize(Roles = "Teacher")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<StudentDto>> GetStudent(int id)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                _logger.LogInformation("GetStudent {StudentId} for teacher {TeacherId}", id, teacherId);
                var student = await _studentService.GetStudentByIdAsync(id, teacherId);
                return Ok(student);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching student {StudentId}", id);
                return StatusCode(500, new { message = "Internal server error while fetching student" });
            }
        }

        /// <summary>Creates a new student under the authenticated teacher.</summary>
        /// <param name="dto">Student creation data. TeacherId is taken from the JWT — not from the request body.</param>
        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<ActionResult<StudentDto>> CreateStudent([FromBody] CreateStudentDto dto)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                _logger.LogInformation("CreateStudent for teacher {TeacherId}", teacherId);
                var student = await _studentService.CreateStudentAsync(dto, teacherId);
                _ = _auditLog.LogAsync("Student", student.Id, "Create", null,
                    JsonSerializer.Serialize(new { student.StudentUniqueId, student.Email, student.GradeId }),
                    teacherId.ToString(), "Teacher");
                return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student");
                return StatusCode(500, new { message = "Internal server error while creating student" });
            }
        }

        /// <summary>Updates an existing student owned by the authenticated teacher.</summary>
        /// <param name="id">The student's primary key.</param>
        /// <param name="dto">Updated student data.</param>
        [Authorize(Roles = "Teacher")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto dto)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                _logger.LogInformation("UpdateStudent {StudentId} for teacher {TeacherId}", id, teacherId);
                var before = await _studentService.GetStudentByIdAsync(id, teacherId);
                var student = await _studentService.UpdateStudentAsync(id, dto, teacherId);
                _ = _auditLog.LogAsync("Student", id, "Update",
                    JsonSerializer.Serialize(new { before.Email, before.GradeId, before.Phone }),
                    JsonSerializer.Serialize(new { student.Email, student.GradeId, student.Phone }),
                    teacherId.ToString(), "Teacher");
                return Ok(student);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student {StudentId}", id);
                return StatusCode(500, new { message = "Internal server error while updating student" });
            }
        }

        /// <summary>Deletes a student owned by the authenticated teacher.</summary>
        /// <param name="id">The student's primary key.</param>
        [Authorize(Roles = "Teacher")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                _logger.LogInformation("DeleteStudent {StudentId} for teacher {TeacherId}", id, teacherId);
                var before = await _studentService.GetStudentByIdAsync(id, teacherId);
                await _studentService.DeleteStudentAsync(id, teacherId);
                _ = _auditLog.LogAsync("Student", id, "Delete",
                    JsonSerializer.Serialize(new { before.StudentUniqueId, before.Email }),
                    null, teacherId.ToString(), "Teacher");
                return Ok(new { message = $"Student with ID {id} successfully deleted" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student {StudentId}", id);
                return StatusCode(500, new { message = "Internal server error while deleting student" });
            }
        }

        // ── Teacher assignment management ─────────────────────────────────────

        /// <summary>Assigns the authenticated teacher to an existing student (many-to-many).</summary>
        /// <param name="studentId">The student's primary key.</param>
        [Authorize(Roles = "Teacher")]
        [HttpPost("{studentId:int}/teachers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignTeacher(int studentId)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                _logger.LogInformation("AssignTeacher: teacher {TeacherId} → student {StudentId}", teacherId, studentId);
                await _studentService.AssignStudentToTeacherAsync(studentId, teacherId);
                return Ok(new { message = $"You have been assigned to student {studentId}." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning teacher {TeacherId} to student {StudentId}", teacherId, studentId);
                return StatusCode(500, new { message = "Internal server error while assigning teacher to student." });
            }
        }

        /// <summary>Removes the authenticated teacher's assignment from a student.</summary>
        /// <param name="studentId">The student's primary key.</param>
        [Authorize(Roles = "Teacher")]
        [HttpDelete("{studentId:int}/teachers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnassignTeacher(int studentId)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                _logger.LogInformation("UnassignTeacher: teacher {TeacherId} → student {StudentId}", teacherId, studentId);
                await _studentService.UnassignStudentFromTeacherAsync(studentId, teacherId);
                return Ok(new { message = $"You have been unassigned from student {studentId}." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unassigning teacher {TeacherId} from student {StudentId}", teacherId, studentId);
                return StatusCode(500, new { message = "Internal server error while unassigning teacher from student." });
            }
        }

        // ── Student self-service (no auth required) ───────────────────────────

        /// <summary>
        /// Activates a student account by linking the student's UniqueId and email to an initial password.
        /// No authentication required — this is the student's first-time self-service setup.
        /// </summary>
        /// <param name="dto">Activation credentials.</param>
        [HttpPost("activate")]
        public async Task<IActionResult> ActivateStudent([FromBody] StudentActivateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                _logger.LogInformation("POST /api/students/activate for {UniqueId}", dto.StudentUniqueId);
                var result = await _studentService.ActivateStudentAsync(dto);
                return result is null ? Unauthorized(new { message = "No student found with that ID and email combination." }) : Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating student {UniqueId}", dto.StudentUniqueId);
                return StatusCode(500, new { message = "Internal server error during account activation." });
            }
        }

        /// <summary>
        /// Authenticates a student by UniqueId and password.
        /// No authentication required — this is the student self-service login.
        /// </summary>
        /// <param name="dto">Student login credentials.</param>
        [HttpPost("login")]
        public async Task<IActionResult> LoginStudent([FromBody] StudentLoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                _logger.LogInformation("POST /api/students/login for {UniqueId}", dto.StudentUniqueId);
                var result = await _studentService.LoginStudentAsync(dto);
                return result is null ? Unauthorized(new { message = "Invalid Student ID or password." }) : Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in student {UniqueId}", dto.StudentUniqueId);
                return StatusCode(500, new { message = "Internal server error during student login." });
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Extracts the teacherId claim from the validated JWT.
        /// Returns false when the claim is missing or non-numeric (caller should 401).
        /// </summary>
        private bool TryGetTeacherId(out int teacherId)
        {
            teacherId = 0;
            var value = User.FindFirstValue("teacherId");
            return value != null && int.TryParse(value, out teacherId);
        }
    }
}
