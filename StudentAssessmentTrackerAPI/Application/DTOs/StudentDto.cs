namespace StudentAssessmentTracker.Application.DTOs
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string? StudentUniqueId { get; set; }
        public string? IdPassportNo { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int GradeId { get; set; }
        public string? GradeName { get; set; }
        public int TeacherId { get; set; }
        public IEnumerable<StudentAssessmentDto> Assessments { get; set; } = new List<StudentAssessmentDto>();
        public decimal TotalScore { get; set; }
        public decimal MaxPossible { get; set; }
        public decimal AverageScore { get; set; }
        public decimal Percentage { get; set; }
        public string? PerformanceLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateStudentDto
    {
        public string? IdPassportNo { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int GradeId { get; set; }
        public int TeacherId { get; set; }
    }

    public class UpdateStudentDto
    {
        public string? IdPassportNo { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int GradeId { get; set; }
    }
}
