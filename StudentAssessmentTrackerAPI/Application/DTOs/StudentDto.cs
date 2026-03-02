namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>
    /// DTO for returning full student data with calculated fields
    /// Used in GET responses
    /// </summary>
    public class StudentDto
    {
        /// <summary>
        /// Unique identifier for the student
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Student's first name
        /// </summary>
        public string? FirstName { get; set; }
        /// <summary>
        /// Student's last name
        /// </summary>
        public string? LastName { get; set; }
        /// <summary>
        /// Student's email address
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// Student's phone number
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// Student's grade or class level
        /// </summary>
        public string? Grade { get; set; }
        /// <summary>
        /// Score for first assessment (0-20)
        /// </summary>
        public decimal Assessment1 { get; set; }
        /// <summary>
        /// Score for second assessment (0-20)
        /// </summary>
        public decimal Assessment2 { get; set; }
        /// <summary>
        /// Score for third assessment (0-20)
        /// </summary>
        public decimal Assessment3 { get; set; }
        /// <summary>
        /// Total score from all assessments
        /// </summary>
        public decimal TotalScore { get; set; }
        /// <summary>
        /// Average score across all assessments
        /// </summary>
        public decimal AverageScore { get; set; }
        /// <summary>
        /// Performance percentage
        /// </summary>
        public decimal Percentage { get; set; }
        /// <summary>
        /// Performance level classification
        /// </summary>
        public string? PerformanceLevel { get; set; }
        /// <summary>
        /// Date when student record was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Date when student record was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a new student
    /// Only requires input fields, excludes calculated fields
    /// </summary>
    public class CreateStudentDto
    {
        /// <summary>
        /// Student's first name
        /// </summary>
        public string? FirstName { get; set; }
        /// <summary>
        /// Student's last name
        /// </summary>
        public string? LastName { get; set; }
        /// <summary>
        /// Student's email address
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// Student's phone number
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// Student's grade or class level
        /// </summary>
        public string? Grade { get; set; }
        /// <summary>
        /// Score for first assessment (0-20)
        /// </summary>
        public decimal Assessment1 { get; set; }
        /// <summary>
        /// Score for second assessment (0-20)
        /// </summary>
        public decimal Assessment2 { get; set; }
        /// <summary>
        /// Score for third assessment (0-20)
        /// </summary>
        public decimal Assessment3 { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing student
    /// Same as CreateStudentDto but semantically for updates
    /// </summary>
    public class UpdateStudentDto
    {
        /// <summary>
        /// Student's first name
        /// </summary>
        public string? FirstName { get; set; }
        /// <summary>
        /// Student's last name
        /// </summary>
        public string? LastName { get; set; }
        /// <summary>
        /// Student's email address
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// Student's phone number
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// Student's grade or class level
        /// </summary>
        public string? Grade { get; set; }
        /// <summary>
        /// Score for first assessment (0-20)
        /// </summary>
        public decimal Assessment1 { get; set; }
        /// <summary>
        /// Score for second assessment (0-20)
        /// </summary>
        public decimal Assessment2 { get; set; }
        /// <summary>
        /// Score for third assessment (0-20)
        /// </summary>
        public decimal Assessment3 { get; set; }
    }
}
