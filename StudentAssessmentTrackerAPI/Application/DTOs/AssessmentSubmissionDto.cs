namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>
    /// Read-only DTO returned when listing or downloading a student file submission.
    /// </summary>
    public class AssessmentSubmissionDto
    {
        /// <summary>Primary key of the submission record.</summary>
        public int Id { get; set; }

        /// <summary>FK to the parent StudentAssessment row.</summary>
        public int StudentAssessmentId { get; set; }

        /// <summary>FK to the student who submitted the file.</summary>
        public int StudentId { get; set; }

        /// <summary>Original filename as provided by the student.</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>MIME type of the uploaded file.</summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>File size in bytes.</summary>
        public long FileSize { get; set; }

        /// <summary>UTC timestamp when the submission was received.</summary>
        public DateTime SubmittedAt { get; set; }
    }
}
