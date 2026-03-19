namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>
    /// Read-only DTO for the Grades lookup table.
    /// Returned by GET /api/grades so the frontend can populate a dropdown.
    /// </summary>
    public class GradeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
    }
}
