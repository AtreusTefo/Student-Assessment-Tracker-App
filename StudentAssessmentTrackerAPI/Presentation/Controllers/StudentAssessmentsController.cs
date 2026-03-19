using Microsoft.AspNetCore.Mvc;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Application.Services;

namespace StudentAssessmentTracker.Presentation.Controllers
{
    /// <summary>
    /// Manages assessments for a specific student.
    /// Nested route under /api/students/{studentId}/assessments allows
    /// updating a single score without touching the student record.
    /// </summary>
    [ApiController]
    [Route("api/students/{studentId:int}/assessments")]
    public class StudentAssessmentsController : ControllerBase
    {
        private readonly IStudentAssessmentService _service;
        private readonly ILogger<StudentAssessmentsController> _logger;

        public StudentAssessmentsController(
            IStudentAssessmentService service,
            ILogger<StudentAssessmentsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>Get all assessments for a student</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<StudentAssessmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<StudentAssessmentDto>>> GetAll(int studentId)
        {
            try
            {
                var result = await _service.GetByStudentIdAsync(studentId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assessments for student {StudentId}", studentId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>Get a single assessment by ID</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(StudentAssessmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentAssessmentDto>> GetById(int studentId, int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(studentId, id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assessment {AssessmentId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>Add a new assessment to a student</summary>
        [HttpPost]
        [ProducesResponseType(typeof(StudentAssessmentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentAssessmentDto>> Create(int studentId, [FromBody] CreateStudentAssessmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var result = await _service.AddAsync(studentId, dto);
                return CreatedAtAction(nameof(GetById), new { studentId, id = result.Id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assessment for student {StudentId}", studentId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>Update a single assessment score — no need to touch the full student record</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(StudentAssessmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentAssessmentDto>> Update(int studentId, int id, [FromBody] UpdateStudentAssessmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var result = await _service.UpdateAsync(studentId, id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assessment {AssessmentId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>Delete an assessment record</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int studentId, int id)
        {
            try
            {
                await _service.DeleteAsync(studentId, id);
                return Ok(new { message = $"Assessment {id} deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assessment {AssessmentId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
