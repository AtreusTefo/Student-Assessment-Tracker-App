using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StudentAssessmentTracker.Application.Services;

namespace StudentAssessmentTracker.Presentation.Controllers
{
    /// <summary>
    /// REST API controller for exporting student reports to CSV and PDF.
    /// All endpoints require a valid Teacher JWT.
    /// </summary>
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Teacher")]
    public class ReportsController : ControllerBase
    {
        private readonly IExportService _exportService;
        private readonly ILogger<ReportsController> _logger;

        /// <summary>Initialises the controller.</summary>
        public ReportsController(IExportService exportService, ILogger<ReportsController> logger)
        {
            _exportService = exportService;
            _logger = logger;
        }

        /// <summary>
        /// Exports ALL students owned by the authenticated teacher to CSV.
        /// </summary>
        /// <returns>A CSV file containing student summary rows.</returns>
        [HttpGet("students/csv")]
        [Produces("text/csv")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExportAllStudentsCsv()
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });

            _logger.LogInformation("Exporting all students CSV for teacher {TeacherId}", teacherId);
            var bytes = await _exportService.ExportStudentsToCsvAsync(teacherId);
            return File(bytes, "text/csv", $"students-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        /// <summary>
        /// Exports a single student's assessment report to CSV.
        /// </summary>
        /// <param name="studentId">The student's primary key.</param>
        [HttpGet("students/{studentId:int}/csv")]
        [Produces("text/csv")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportStudentReportCsv(int studentId)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                _logger.LogInformation("Exporting CSV report for student {StudentId}", studentId);
                var bytes = await _exportService.ExportStudentReportToCsvAsync(studentId, teacherId);
                return File(bytes, "text/csv", $"student-{studentId}-report-{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        /// <summary>
        /// Exports a single student's assessment report to PDF.
        /// </summary>
        /// <param name="studentId">The student's primary key.</param>
        [HttpGet("students/{studentId:int}/pdf")]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportStudentReportPdf(int studentId)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid or missing token." });

            try
            {
                _logger.LogInformation("Exporting PDF report for student {StudentId}", studentId);
                var bytes = await _exportService.ExportStudentReportToPdfAsync(studentId, teacherId);
                return File(bytes, "application/pdf", $"student-{studentId}-report-{DateTime.UtcNow:yyyyMMdd}.pdf");
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
