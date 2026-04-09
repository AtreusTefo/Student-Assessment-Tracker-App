using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Application.Services;

namespace StudentAssessmentTracker.Presentation.Controllers
{
    /// <summary>
    /// REST API controller for managing student file submissions against scheduled assessments.
    /// Route: /api/students/{studentId}/assessments/{assessmentId}/submissions
    /// </summary>
    [ApiController]
    [Route("api/students/{studentId:int}/assessments/{assessmentId:int}/submissions")]
    public class AssessmentSubmissionsController : ControllerBase
    {
        private readonly IAssessmentSubmissionService _service;
        private readonly ILogger<AssessmentSubmissionsController> _logger;

        /// <summary>Initialises the controller with the submission service and logger.</summary>
        public AssessmentSubmissionsController(
            IAssessmentSubmissionService service,
            ILogger<AssessmentSubmissionsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ====================================================================
        // POST /api/students/{studentId}/assessments/{assessmentId}/submissions
        // Student JWT only — upload a submission file
        // ====================================================================

        /// <summary>Upload a submission file for an assessment (student only).</summary>
        /// <param name="studentId">The student's primary key (must match the authenticated student).</param>
        /// <param name="assessmentId">The target assessment's primary key.</param>
        /// <param name="file">The uploaded file (pdf/doc/docx/jpg/jpeg/png, max 10 MB).</param>
        /// <response code="201">Submission accepted and stored.</response>
        /// <response code="400">Invalid file type or size.</response>
        /// <response code="401">Not authenticated as the correct student.</response>
        /// <response code="404">Assessment not found for this student.</response>
        [Authorize(Roles = "Student")]
        [HttpPost]
        [ProducesResponseType(typeof(AssessmentSubmissionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Upload(int studentId, int assessmentId, IFormFile file)
        {
            if (!TryGetStudentId(out var callerId))
                return Unauthorized(new { message = "Invalid or missing token." });

            if (callerId != studentId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You may only submit files for your own assessments." });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file was provided." });

            try
            {
                var result = await _service.SubmitAsync(studentId, assessmentId, file);
                return CreatedAtAction(nameof(GetSubmissions),
                    new { studentId, assessmentId }, result);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading submission for student {StudentId}", studentId);
                return StatusCode(500, new { message = "Internal server error during file upload." });
            }
        }

        // ====================================================================
        // GET /api/students/{studentId}/assessments/{assessmentId}/submissions
        // Teacher JWT only — list submissions for a student's assessment
        // ====================================================================

        /// <summary>List all submissions for an assessment (teacher only).</summary>
        /// <param name="studentId">The student's primary key.</param>
        /// <param name="assessmentId">The assessment's primary key.</param>
        /// <response code="200">Submission list returned.</response>
        /// <response code="401">Not authenticated as a teacher.</response>
        [Authorize(Roles = "Teacher")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AssessmentSubmissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSubmissions(int studentId, int assessmentId)
        {
            if (!TryGetTeacherId(out var callerId))
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                var results = await _service.GetSubmissionsAsync(studentId, assessmentId, callerId, isStudent: false);
                return Ok(results);
            }
            catch (UnauthorizedAccessException ex) { return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching submissions for student {StudentId}", studentId);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // ====================================================================
        // GET /api/students/{studentId}/assessments/{assessmentId}/submissions/{id}/download
        // Teacher or Student JWT — download a specific submission file
        // ====================================================================

        /// <summary>Download a submission file (teacher or owning student).</summary>
        /// <param name="studentId">The student's primary key.</param>
        /// <param name="assessmentId">The assessment's primary key.</param>
        /// <param name="id">The submission's primary key.</param>
        /// <response code="200">File stream returned.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Authenticated but not authorised to access this file.</response>
        /// <response code="404">Submission or file not found.</response>
        [Authorize]
        [HttpGet("{id:int}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(int studentId, int assessmentId, int id)
        {
            bool isStudent = TryGetStudentId(out var callerId);
            if (!isStudent)
            {
                if (!TryGetTeacherId(out callerId))
                    return Unauthorized(new { message = "Invalid or missing token." });
            }

            try
            {
                var (data, contentType, fileName) = await _service.DownloadAsync(id, callerId, isStudent);
                return File(data, contentType, fileName);
            }
            catch (UnauthorizedAccessException ex) { return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (FileNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading submission {SubmissionId}", id);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // ====================================================================
        // DELETE /api/students/{studentId}/assessments/{assessmentId}/submissions/{id}
        // Teacher or Student JWT — delete a submission
        // ====================================================================

        /// <summary>Delete a submission (teacher or owning student).</summary>
        /// <param name="studentId">The student's primary key.</param>
        /// <param name="assessmentId">The assessment's primary key.</param>
        /// <param name="id">The submission's primary key.</param>
        /// <response code="200">Submission deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Authenticated but not the owner.</response>
        /// <response code="404">Submission not found.</response>
        [Authorize]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int studentId, int assessmentId, int id)
        {
            bool isStudent = TryGetStudentId(out var callerId);
            // BUG #1 fix: capture the teacher ID so the service can check teacher-student ownership.
            if (!isStudent && !TryGetTeacherId(out callerId))
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                await _service.DeleteSubmissionAsync(id, callerId, isStudent);
                return Ok(new { message = $"Submission {id} deleted successfully." });
            }
            catch (UnauthorizedAccessException ex) { return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting submission {SubmissionId}", id);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Extracts the studentId claim from the validated JWT.</summary>
        private bool TryGetStudentId(out int studentId)
        {
            studentId = 0;
            var value = User.FindFirstValue("studentId");
            return value != null && int.TryParse(value, out studentId);
        }

        /// <summary>Extracts the teacherId claim from the validated JWT.</summary>
        private bool TryGetTeacherId(out int teacherId)
        {
            teacherId = 0;
            var value = User.FindFirstValue("teacherId");
            return value != null && int.TryParse(value, out teacherId);
        }
    }
}
