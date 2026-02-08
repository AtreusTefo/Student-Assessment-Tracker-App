using System;
using System.ComponentModel.DataAnnotations;

namespace StudentAssessmentTracker.Models;

public class Teacher
{
    public int TeacherId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; } = DateTime.Now;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
