namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>
    /// DTO for returning full student data with calculated fields
    /// Used in GET responses
    /// </summary>
    public class StudentDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Grade { get; set; }
        public decimal Assessment1 { get; set; }
        public decimal Assessment2 { get; set; }
        public decimal Assessment3 { get; set; }
        public decimal TotalScore { get; set; }
        public decimal AverageScore { get; set; }
        public decimal Percentage { get; set; }
        public string? PerformanceLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a new student
    /// Only requires input fields, excludes calculated fields
    /// </summary>
    public class CreateStudentDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Grade { get; set; }
        public decimal Assessment1 { get; set; }
        public decimal Assessment2 { get; set; }
        public decimal Assessment3 { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing student
    /// Same as CreateStudentDto but semantically for updates
    /// </summary>
    public class UpdateStudentDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Grade { get; set; }
        public decimal Assessment1 { get; set; }
        public decimal Assessment2 { get; set; }
        public decimal Assessment3 { get; set; }
    }
}
