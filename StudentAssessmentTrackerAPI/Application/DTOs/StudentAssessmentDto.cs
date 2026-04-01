namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>
    /// DTO returned when reading an assessment record
    /// </summary>
    public class StudentAssessmentDto
    {
        /// <summary>Unique identifier of the assessment.</summary>
        public int Id { get; set; }
        /// <summary>ID of the student this assessment belongs to.</summary>
        public int StudentId { get; set; }
        /// <summary>Name/title of the assessment.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Maximum possible score for this assessment.</summary>
        public decimal MaxScore { get; set; }
        /// <summary>Score achieved by the student.</summary>
        public decimal Score { get; set; }
        /// <summary>Optional due date for the assessment.</summary>
        public DateTime? DueDate { get; set; }
        /// <summary>UTC timestamp when the record was created.</summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>UTC timestamp when the record was last updated.</summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for adding a new assessment to a student
    /// </summary>
    public class CreateStudentAssessmentDto
    {
        /// <summary>Name/title of the assessment.</summary>
        public string? Name { get; set; }
        /// <summary>Maximum possible score for this assessment.</summary>
        public decimal MaxScore { get; set; }
        /// <summary>Score achieved by the student.</summary>
        public decimal Score { get; set; }
        /// <summary>Optional due date for the assessment.</summary>
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing assessment record
    /// </summary>
    public class UpdateStudentAssessmentDto
    {
        /// <summary>Name/title of the assessment.</summary>
        public string? Name { get; set; }
        /// <summary>Maximum possible score for this assessment.</summary>
        public decimal MaxScore { get; set; }
        /// <summary>Score achieved by the student.</summary>
        public decimal Score { get; set; }
        /// <summary>Optional due date for the assessment.</summary>
        public DateTime? DueDate { get; set; }
    }
}
