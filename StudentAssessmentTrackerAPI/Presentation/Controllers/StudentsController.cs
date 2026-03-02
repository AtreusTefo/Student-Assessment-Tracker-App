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

        /// <summary>
        /// Initializes a new instance of the StudentsController
        /// </summary>
        /// <param name="studentService">The student service for business logic operations</param>
        /// <param name="logger">Logger for tracking controller operations</param>
        public StudentsController(
            IStudentService studentService,
            ILogger<StudentsController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all students from the database
        /// </summary>
        /// <returns>A collection of StudentDto objects containing student information</returns>
        /// <response code="200">Successfully retrieved all students</response>
        /// <response code="500">Internal server error while fetching students</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<StudentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// Retrieves a specific student by their ID
        /// </summary>
        /// <param name="id">The unique identifier of the student to retrieve</param>
        /// <returns>A StudentDto object containing the requested student's information</returns>
        /// <response code="200">Successfully retrieved the student</response>
        /// <response code="404">Student with the specified ID was not found</response>
        /// <response code="500">Internal server error while fetching the student</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// Creates a new student record
        /// </summary>
        /// <param name="dto">The CreateStudentDto object containing student data to create</param>
        /// <returns>The newly created StudentDto with assigned ID</returns>
        /// <response code="201">Student successfully created</response>
        /// <response code="400">Invalid request data or validation failed</response>
        /// <response code="500">Internal server error while creating the student</response>
        [HttpPost]
        [ProducesResponseType(typeof(StudentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// Updates an existing student's information
        /// </summary>
        /// <param name="id">The unique identifier of the student to update</param>
        /// <param name="dto">The UpdateStudentDto object containing updated student data</param>
        /// <returns>The updated StudentDto object</returns>
        /// <response code="200">Student successfully updated</response>
        /// <response code="400">Invalid request data or validation failed</response>
        /// <response code="404">Student with the specified ID was not found</response>
        /// <response code="500">Internal server error while updating the student</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// Deletes a student from the database
        /// </summary>
        /// <param name="id">The unique identifier of the student to delete</param>
        /// <returns>No content - indicates successful deletion</returns>
        /// <response code="204">Student successfully deleted</response>
        /// <response code="404">Student with the specified ID was not found</response>
        /// <response code="500">Internal server error while deleting the student</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
