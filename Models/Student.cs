using System;
using System.ComponentModel.DataAnnotations;

namespace StudentAssessmentTracker.Models;

public class Student
{
    public int StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; } = DateTime.Now;

    // Each assessment is marked out of 20
    public int Assessment1 { get; set; }
    public int Assessment2 { get; set; }
    public int Assessment3 { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public int Total => Assessment1 + Assessment2 + Assessment3;
    public double Average => Total / 3.0;
    public double Percentage => (Total / 60.0) * 100;

    public string PerformanceLevel
    {
        get
        {
            if (Percentage < 50) return "Needs Support";
            if (Percentage <= 55) return "Satisfactory";
            if (Percentage <= 75) return "Good";
            return "Excellent";
        }
    }
}
