namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Represents a file uploaded by a student as a submission for a specific assessment.
    /// The physical file is stored under wwwroot/uploads/submissions/{studentId}/
    /// using a GUID-based filename to prevent collisions and directory traversal.
    /// </summary>
    public class AssessmentSubmission
    {
        /// <summary>Primary key</summary>
        public int Id { get; set; }

        /// <summary>FK → StudentAssessments.Id (cascade delete)</summary>
        public int StudentAssessmentId { get; set; }

        /// <summary>Navigation to the owning assessment. Always loaded when fetching a submission.</summary>
        public StudentAssessment StudentAssessment { get; set; } = null!;

        /// <summary>Original filename as uploaded by the student (display only)</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>GUID-based filename used for storage on disk</summary>
        public string StoredFileName { get; set; } = string.Empty;

        /// <summary>MIME type of the uploaded file</summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>File size in bytes</summary>
        public long FileSize { get; set; }

        /// <summary>UTC timestamp when the submission was received</summary>
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
