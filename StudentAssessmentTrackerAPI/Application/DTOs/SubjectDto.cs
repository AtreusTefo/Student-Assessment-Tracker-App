namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>DTO returned for a subject lookup record</summary>
    public class SubjectDto
    {
        /// <summary>Gets or sets the unique identifier</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the display name</summary>
        public string Name { get; set; } = string.Empty;
    }
}
