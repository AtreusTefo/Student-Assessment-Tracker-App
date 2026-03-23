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

    /// <summary>Safe public profile returned to the student after login/activation (no password field)</summary>
    public class StudentProfileDto
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

    /// <summary>DTO for student account activation — links StudentUniqueId + Email to a new password</summary>
    public class StudentActivateDto
    {
        public string StudentUniqueId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>DTO for student login credentials</summary>
    public class StudentLoginDto
    {
        public string StudentUniqueId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>DTO returned on successful student login or activation</summary>
    public class StudentLoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public StudentProfileDto Student { get; set; } = new();
    }
}
