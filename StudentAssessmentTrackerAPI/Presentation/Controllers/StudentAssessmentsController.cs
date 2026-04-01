using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Application.Services;

namespace StudentAssessmentTracker.Presentation.Controllers
{
    /// <summary>
    /// REST API controller for assessment operations scoped to a specific student.
    /// All endpoints require a valid teacher JWT — the teacher must own the referenced student.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/students/{studentId:int}/assessments")]
    public class StudentAssessmentsController : ControllerBase
    {
        private readonly IStudentAssessmentService _service;
        private readonly ILogger<StudentAssessmentsController> _logger;

        /// <summary>Initialises the controller with the assessment service and logger.</summary>
        public StudentAssessmentsController(
            IStudentAssessmentService service,
            ILogger<StudentAssessmentsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>Returns all assessments for the specified student.</summary>
        /// <param name="studentId">The student whose assessments are requested.</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<StudentAssessmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<StudentAssessmentDto>>> GetAll(int studentId)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                var result = await _service.GetByStudentIdAsync(studentId, teacherId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assessments for student {StudentId}", studentId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>Returns a single assessment by its ID for the specified student.</summary>
        /// <param name="studentId">The owning student's ID.</param>
        /// <param name="id">The assessment ID.</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(StudentAssessmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentAssessmentDto>> GetById(int studentId, int id)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                var result = await _service.GetByIdAsync(studentId, id, teacherId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assessment {AssessmentId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>Creates a new assessment for the specified student.</summary>
        /// <param name="studentId">The student to attach the assessment to.</param>
        /// <param name="dto">Assessment creation data.</param>
        [HttpPost]
        [ProducesResponseType(typeof(StudentAssessmentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentAssessmentDto>> Create(int studentId, [FromBody] CreateStudentAssessmentDto dto)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var result = await _service.AddAsync(studentId, dto, teacherId);
                return CreatedAtAction(nameof(GetById), new { studentId, id = result.Id }, result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assessment for student {StudentId}", studentId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>Updates an existing assessment for the specified student.</summary>
        /// <param name="studentId">The owning student's ID.</param>
        /// <param name="id">The assessment ID to update.</param>
        /// <param name="dto">Updated assessment data.</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(StudentAssessmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentAssessmentDto>> Update(int studentId, int id, [FromBody] UpdateStudentAssessmentDto dto)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var result = await _service.UpdateAsync(studentId, id, dto, teacherId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assessment {AssessmentId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>Delete an assessment record</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int studentId, int id)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });
            try
            {
                await _service.DeleteAsync(studentId, id, teacherId);
                return Ok(new { message = $"Assessment {id} deleted successfully" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assessment {AssessmentId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private bool TryGetTeacherId(out int teacherId)
        {
            teacherId = 0;
            var value = User.FindFirstValue("teacherId");
            return value != null && int.TryParse(value, out teacherId);
        }
    }
}
