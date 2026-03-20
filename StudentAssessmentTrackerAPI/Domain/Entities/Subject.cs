namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Subject lookup entity — controlled list of subjects that can be assigned to a teacher.
    /// Seeded at startup; teachers cannot create new subjects.
    /// </summary>
    public class Subject
    {
        /// <summary>Gets or sets the unique identifier</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the display name of the subject</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Teachers assigned to this subject</summary>
        public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    }
}
