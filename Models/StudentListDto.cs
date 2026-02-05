namespace StudentAssessmentTracker.Models;

/// <summary>
/// DTO (Data Transfer Object) for displaying student list.
/// Only exposes Id, FirstName, and LastName — hides sensitive properties like Email and Phone.
/// AutoMapper will automatically map Student → StudentListDto by matching property names.
/// </summary>
public class StudentListDto
{
    public int StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
