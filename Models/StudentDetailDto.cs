namespace StudentAssessmentTracker.Models;

/// <summary>
/// DTO (Data Transfer Object) for displaying detailed student information.
/// Includes assessment scores, calculations, and performance level.
/// AutoMapper will automatically map Student → StudentDetailDto.
/// </summary>
public class StudentDetailDto
{
    public int StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }

    // Assessment scores
    public int Assessment1 { get; set; }
    public int Assessment2 { get; set; }
    public int Assessment3 { get; set; }

    // Calculated fields
    public int Total { get; set; }
    public double Average { get; set; }
    public double Percentage { get; set; }
    public string PerformanceLevel { get; set; } = string.Empty;
}
