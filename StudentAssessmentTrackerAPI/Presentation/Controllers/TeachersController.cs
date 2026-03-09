using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<TeachersController> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="TeachersController"/>
        /// </summary>
        /// <param name="teacherService">Teacher application service</param>
        /// <param name="logger">Logger instance</param>
        public TeachersController(ITeacherService teacherService, ILogger<TeachersController> logger)
        {
            _teacherService = teacherService;
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
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TeacherResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("GET /api/teachers");
            var teachers = await _teacherService.GetAllTeachersAsync();
            return Ok(teachers);
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
        /// <response code="404">Teacher not found</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TeacherResponseDto), StatusCodes.Status200OK)]
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
        public async Task<IActionResult> Create([FromBody] TeacherRegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("POST /api/teachers — registering {Email}", dto.Email);
            var created = await _teacherService.CreateTeacherAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.TeacherId }, created);
        }

        // ====================================================================
        // PUT /api/teachers/{id}
        // ====================================================================

        /// <summary>
        /// Updates an existing teacher
        /// </summary>
        /// <param name="id">The teacher identifier</param>
        /// <param name="dto">Updated teacher data</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Teacher updated successfully</response>
        /// <response code="404">Teacher not found</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] TeacherUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("PUT /api/teachers/{Id}", id);
            var updated = await _teacherService.UpdateTeacherAsync(id, dto);
            return updated ? NoContent() : NotFound(new { message = $"Teacher with ID {id} not found." });
        }

        // ====================================================================
        // DELETE /api/teachers/{id}
        // ====================================================================

        /// <summary>
        /// Deletes a teacher by ID
        /// </summary>
        /// <param name="id">The teacher identifier</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Teacher deleted successfully</response>
        /// <response code="404">Teacher not found</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("DELETE /api/teachers/{Id}", id);
            var deleted = await _teacherService.DeleteTeacherAsync(id);
            return deleted ? NoContent() : NotFound(new { message = $"Teacher with ID {id} not found." });
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
    }
}
