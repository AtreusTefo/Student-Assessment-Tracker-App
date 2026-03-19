namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Represents a single assessment result for a student.
    /// Extracted from the Students row so each assessment is independently named,
    /// scored, and dated — fixing the fixed-count, hardcoded-max, and tied-row problems.
    /// </summary>
    public class StudentAssessment
    {
        /// <summary>Primary key</summary>
        public int Id { get; set; }

        /// <summary>FK → Students.Id (cascade delete)</summary>
        public int StudentId { get; set; }

        /// <summary>Navigation property back to owner student</summary>
        public Student Student { get; set; } = null!;

        /// <summary>Descriptive name set by the teacher, e.g., "Test 1", "Assignment 2", "Final Exam"</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Maximum possible mark for this assessment — set by the teacher.
        /// Allows any scale (20, 50, 100, etc.) instead of a hardcoded system-wide limit.
        /// </summary>
        public decimal MaxScore { get; set; }

        /// <summary>Actual score achieved. Must be &gt;= 0 and &lt;= MaxScore.</summary>
        public decimal Score { get; set; }

        /// <summary>Optional due / sitting date for this assessment</summary>
        public DateTime? DueDate { get; set; }

        /// <summary>Record creation timestamp</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Record last-updated timestamp</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
