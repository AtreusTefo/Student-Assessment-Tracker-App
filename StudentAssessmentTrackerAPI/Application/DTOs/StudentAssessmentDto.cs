namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>
    /// DTO returned when reading an assessment record
    /// </summary>
    public class StudentAssessmentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal MaxScore { get; set; }
        public decimal Score { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for adding a new assessment to a student
    /// </summary>
    public class CreateStudentAssessmentDto
    {
        public string? Name { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Score { get; set; }
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing assessment record
    /// </summary>
    public class UpdateStudentAssessmentDto
    {
        public string? Name { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Score { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
