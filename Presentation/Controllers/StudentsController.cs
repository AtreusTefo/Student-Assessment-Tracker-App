using Microsoft.AspNetCore.Mvc;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Application.Services;

namespace StudentAssessmentTracker.Presentation.Controllers
{
    /// <summary>
    /// REST API Controller for Student operations
    /// Presentation layer - handles HTTP requests and responses
    /// Delegates business logic to IStudentService
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(
            IStudentService studentService,
            ILogger<StudentsController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/students
        /// Retrieves all students
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetAllStudents()
        {
            try
            {
                _logger.LogInformation("GetAllStudents endpoint called");
                var students = await _studentService.GetAllStudentsAsync();
                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching students");
                return StatusCode(500, new { message = "Internal server error while fetching students" });
            }
        }

        /// <summary>
        /// GET /api/students/{id}
        /// Retrieves a specific student by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDto>> GetStudent(int id)
        {
            try
            {
                _logger.LogInformation("GetStudent endpoint called with ID: {StudentId}", id);
                var student = await _studentService.GetStudentByIdAsync(id);
                return Ok(student);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching student with ID {StudentId}", id);
                return StatusCode(500, new { message = "Internal server error while fetching student" });
            }
        }

        /// <summary>
        /// POST /api/students
        /// Creates a new student
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<StudentDto>> CreateStudent([FromBody] CreateStudentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("CreateStudent endpoint called");
                var student = await _studentService.CreateStudentAsync(dto);
                return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student");
                return StatusCode(500, new { message = "Internal server error while creating student" });
            }
        }

        /// <summary>
        /// PUT /api/students/{id}
        /// Updates an existing student
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("UpdateStudent endpoint called with ID: {StudentId}", id);
                var student = await _studentService.UpdateStudentAsync(id, dto);
                return Ok(student);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student with ID {StudentId}", id);
                return StatusCode(500, new { message = "Internal server error while updating student" });
            }
        }

        /// <summary>
        /// DELETE /api/students/{id}
        /// Deletes a student by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                _logger.LogInformation("DeleteStudent endpoint called with ID: {StudentId}", id);
                await _studentService.DeleteStudentAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student with ID {StudentId}", id);
                return StatusCode(500, new { message = "Internal server error while deleting student" });
            }
        }
    }
}
