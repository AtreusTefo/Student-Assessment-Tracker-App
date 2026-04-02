using AutoMapper;
using Microsoft.AspNetCore.Http;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>
    /// Contract for the assessment-submission application service.
    /// </summary>
    public interface IAssessmentSubmissionService
    {
        /// <summary>
        /// Accepts an uploaded file, validates it, writes it to disk, and persists the metadata row.
        /// </summary>
        Task<AssessmentSubmissionDto> SubmitAsync(int studentId, int assessmentId, IFormFile file);

        /// <summary>
        /// Returns submission records for a particular assessment.
        /// Students may only see their own; teachers may see all for students they own.
        /// </summary>
        Task<IEnumerable<AssessmentSubmissionDto>> GetSubmissionsAsync(
            int studentId, int assessmentId, int callerId, bool isStudent);

        /// <summary>
        /// Returns the raw bytes, MIME type, and original filename for a submission file.
        /// </summary>
        Task<(byte[] Data, string ContentType, string FileName)> DownloadAsync(
            int submissionId, int callerId, bool isStudent);

        /// <summary>
        /// Permanently deletes a submission record and its on-disk file.
        /// </summary>
        Task DeleteSubmissionAsync(int submissionId, int studentId);
    }

    /// <summary>
    /// Application-layer service handling file-upload business logic for assessment submissions.
    /// </summary>
    public class AssessmentSubmissionService : IAssessmentSubmissionService
    {
        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        private readonly IAssessmentSubmissionRepository _submissionRepo;
        private readonly IStudentAssessmentRepository _assessmentRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AssessmentSubmissionService> _logger;

        /// <summary>Initialises the service with its dependencies.</summary>
        public AssessmentSubmissionService(
            IAssessmentSubmissionRepository submissionRepo,
            IStudentAssessmentRepository assessmentRepo,
            IStudentRepository studentRepo,
            IMapper mapper,
            IWebHostEnvironment env,
            ILogger<AssessmentSubmissionService> logger)
        {
            _submissionRepo = submissionRepo;
            _assessmentRepo = assessmentRepo;
            _studentRepo = studentRepo;
            _mapper = mapper;
            _env = env;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<AssessmentSubmissionDto> SubmitAsync(int studentId, int assessmentId, IFormFile file)
        {
            // ── Validate file ────────────────────────────────────────────────
            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException(
                    $"File type '{extension}' is not allowed. Allowed types: pdf, doc, docx, jpg, jpeg, png.");

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException("File exceeds the 10 MB maximum size limit.");

            // ── Verify assessment belongs to student ─────────────────────────
            var assessment = await _assessmentRepo.GetByIdForStudentAsync(studentId, assessmentId);
            if (assessment is null)
                throw new KeyNotFoundException($"Assessment {assessmentId} not found for student {studentId}.");

            // ── Build storage path ────────────────────────────────────────────
            var submissionsRoot = Path.Combine(_env.WebRootPath, "uploads", "submissions", studentId.ToString());
            Directory.CreateDirectory(submissionsRoot);

            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(submissionsRoot, storedFileName);

            // ── Write file then persist DB row atomically ─────────────────────
            try
            {
                await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write submission file for student {StudentId}", studentId);
                throw;
            }

            var submission = new AssessmentSubmission
            {
                StudentAssessmentId = assessmentId,
                StudentId = studentId,
                FileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                SubmittedAt = DateTime.UtcNow
            };

            try
            {
                await _submissionRepo.AddAsync(submission);
            }
            catch (Exception ex)
            {
                // Rollback: remove the file that was written if the DB persist fails
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                _logger.LogError(ex, "DB persist failed for submission of student {StudentId}; file rolled back", studentId);
                throw;
            }

            _logger.LogInformation(
                "Submission {SubmissionId} saved for student {StudentId}, assessment {AssessmentId}",
                submission.Id, studentId, assessmentId);

            return _mapper.Map<AssessmentSubmissionDto>(submission);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssessmentSubmissionDto>> GetSubmissionsAsync(
            int studentId, int assessmentId, int callerId, bool isStudent)
        {
            if (isStudent && callerId != studentId)
                throw new UnauthorizedAccessException("Students may only view their own submissions.");

            var submissions = await _submissionRepo.GetByAssessmentAndStudentAsync(assessmentId, studentId);
            return _mapper.Map<IEnumerable<AssessmentSubmissionDto>>(submissions);
        }

        /// <inheritdoc />
        public async Task<(byte[] Data, string ContentType, string FileName)> DownloadAsync(
            int submissionId, int callerId, bool isStudent)
        {
            var submission = await _submissionRepo.GetByIdAsync(submissionId);
            if (submission is null)
                throw new KeyNotFoundException($"Submission {submissionId} not found.");

            if (isStudent && callerId != submission.StudentId)
                throw new UnauthorizedAccessException("Students may only download their own submissions.");

            var filePath = Path.Combine(
                _env.WebRootPath, "uploads", "submissions",
                submission.StudentId.ToString(), submission.StoredFileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Submission file not found on disk.", filePath);

            var data = await File.ReadAllBytesAsync(filePath);
            return (data, submission.ContentType, submission.FileName);
        }

        /// <inheritdoc />
        public async Task DeleteSubmissionAsync(int submissionId, int studentId)
        {
            var submission = await _submissionRepo.GetByIdAsync(submissionId);
            if (submission is null)
                throw new KeyNotFoundException($"Submission {submissionId} not found.");

            if (submission.StudentId != studentId)
                throw new UnauthorizedAccessException("You may only delete your own submissions.");

            // Delete physical file first, then DB row
            var filePath = Path.Combine(
                _env.WebRootPath, "uploads", "submissions",
                submission.StudentId.ToString(), submission.StoredFileName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            await _submissionRepo.DeleteAsync(submission);

            _logger.LogInformation(
                "Submission {SubmissionId} deleted by student {StudentId}", submissionId, studentId);
        }
    }
}
