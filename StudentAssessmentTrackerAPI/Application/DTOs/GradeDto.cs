namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>
    /// Read-only DTO for the Grades lookup table.
    /// Returned by GET /api/grades so the frontend can populate a dropdown.
    /// </summary>
    public class GradeDto
    {
        /// <summary>Unique identifier of the grade.</summary>
        public int Id { get; set; }
        /// <summary>Display name of the grade (e.g. "Grade 7").</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Numeric level of the grade (e.g. 7 for Grade 7).</summary>
        public int Level { get; set; }
    }
}
