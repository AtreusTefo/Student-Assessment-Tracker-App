using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Application.Services;

namespace StudentAssessmentTracker.Presentation.Controllers
{
    /// <summary>
    /// REST API controller for class group management.
    /// All endpoints require a Teacher JWT.
    /// </summary>
    [ApiController]
    [Route("api/class-groups")]
    [Authorize(Roles = "Teacher")]
    [Produces("application/json")]
    public class ClassGroupsController : ControllerBase
    {
        private readonly IClassGroupService _classGroupService;
        private readonly IAuditLogService _auditLog;
        private readonly ILogger<ClassGroupsController> _logger;

        /// <summary>Initialises the controller.</summary>
        public ClassGroupsController(
            IClassGroupService classGroupService,
            IAuditLogService auditLog,
            ILogger<ClassGroupsController> logger)
        {
            _classGroupService = classGroupService;
            _auditLog = auditLog;
            _logger = logger;
        }

        /// <summary>Returns all class groups owned by the authenticated teacher.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClassGroupDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });

            var groups = await _classGroupService.GetAllByTeacherAsync(teacherId);
            return Ok(groups);
        }

        /// <summary>Returns a single class group by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ClassGroupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                var group = await _classGroupService.GetByIdAsync(id, teacherId);
                return Ok(group);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        /// <summary>Creates a new class group under the authenticated teacher.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ClassGroupDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateClassGroupDto dto)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var group = await _classGroupService.CreateAsync(dto, teacherId);
                await _auditLog.LogAsync("ClassGroup", group.Id, "Create", null,
                    $"{{\"name\":\"{group.Name}\",\"subjectId\":{group.SubjectId},\"gradeId\":{group.GradeId}}}",
                    teacherId.ToString(), "Teacher");
                return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Updates the name of an existing class group.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ClassGroupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClassGroupDto dto)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var group = await _classGroupService.UpdateAsync(id, dto, teacherId);
                await _auditLog.LogAsync("ClassGroup", id, "Update", null,
                    $"{{\"name\":\"{group.Name}\"}}",
                    teacherId.ToString(), "Teacher");
                return Ok(group);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        /// <summary>Deletes a class group and all its enrollments.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                await _classGroupService.DeleteAsync(id, teacherId);
                await _auditLog.LogAsync("ClassGroup", id, "Delete",
                    $"{{\"id\":{id}}}", null, teacherId.ToString(), "Teacher");
                return Ok(new { message = $"Class group {id} deleted." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ── Enrollment management ─────────────────────────────────────────────

        /// <summary>Enrolls a student in the class group.</summary>
        [HttpPost("{id:int}/students")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EnrollStudent(int id, [FromBody] ClassGroupEnrollDto dto)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                await _classGroupService.EnrollStudentAsync(id, dto.StudentId, teacherId);
                return Ok(new { message = $"Student {dto.StudentId} enrolled in class group {id}." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        /// <summary>Removes a student from the class group.</summary>
        [HttpDelete("{id:int}/students/{studentId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnenrollStudent(int id, int studentId)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                await _classGroupService.UnenrollStudentAsync(id, studentId, teacherId);
                return Ok(new { message = $"Student {studentId} unenrolled from class group {id}." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private bool TryGetTeacherId(out int teacherId)
        {
            teacherId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
            return claim is not null && int.TryParse(claim, out teacherId) && teacherId > 0;
        }
    }
}
