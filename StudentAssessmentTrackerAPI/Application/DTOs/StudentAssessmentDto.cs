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
        /// <summary>When true, the student is expected to submit a file for this assessment.</summary>
        public bool IsAssigned { get; set; }
        /// <summary>Optional teacher instructions shown to the student.</summary>
        public string? Instructions { get; set; }
        /// <summary>Number of file submissions uploaded by the student for this assessment.</summary>
        public int SubmissionCount { get; set; }
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
        /// <summary>Whether the student should submit a file for this assessment.</summary>
        public bool? IsAssigned { get; set; }
        /// <summary>Optional teacher instructions for the student.</summary>
        public string? Instructions { get; set; }
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
        /// <summary>Whether the student should submit a file for this assessment.</summary>
        public bool? IsAssigned { get; set; }
        /// <summary>Optional teacher instructions for the student.</summary>
        public string? Instructions { get; set; }
    }

    /// <summary>
    /// DTO for assigning the same assessment to multiple students at once.
    /// Each student in <see cref="StudentIds"/> receives an identical assessment record.
    /// </summary>
    public class BulkCreateStudentAssessmentDto
    {
        /// <summary>Name/title of the assessment.</summary>
        public string? Name { get; set; }
        /// <summary>Maximum possible score for this assessment.</summary>
        public decimal MaxScore { get; set; }
        /// <summary>Score starts at 0 for all created assessments.</summary>
        public decimal Score { get; set; }
        /// <summary>Optional due date for the assessment.</summary>
        public DateTime? DueDate { get; set; }
        /// <summary>Whether the students should submit files for this assessment.</summary>
        public bool? IsAssigned { get; set; }
        /// <summary>Optional teacher instructions for the students.</summary>
        public string? Instructions { get; set; }
        /// <summary>IDs of the students who should receive this assessment.</summary>
        public List<int> StudentIds { get; set; } = new();
    }
}
