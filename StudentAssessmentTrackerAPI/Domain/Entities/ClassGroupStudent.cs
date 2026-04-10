namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Many-to-many join entity linking Students to ClassGroups.
    /// A student may be enrolled in multiple class groups (e.g., Maths and English),
    /// and a class group may contain many students.
    /// </summary>
    public class ClassGroupStudent
    {
        /// <summary>FK → ClassGroups.Id.</summary>
        public int ClassGroupId { get; set; }

        /// <summary>Navigation property for the parent class group.</summary>
        public ClassGroup? ClassGroup { get; set; }

        /// <summary>FK → Students.Id.</summary>
        public int StudentId { get; set; }

        /// <summary>Navigation property for the enrolled student.</summary>
        public Student? Student { get; set; }

        /// <summary>UTC timestamp when the student was enrolled in this class group.</summary>
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}
