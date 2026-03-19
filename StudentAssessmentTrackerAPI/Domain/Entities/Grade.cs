namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Grade lookup entity — seeded with fixed school grade levels (Grade 7–12).
    /// Students reference this table via GradeId FK, preventing free-text inconsistency.
    /// </summary>
    public class Grade
    {
        /// <summary>Primary key</summary>
        public int Id { get; set; }

        /// <summary>Display label, e.g., "Grade 7"</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Numeric grade level for sorting and range validation</summary>
        public int Level { get; set; }
    }
}
