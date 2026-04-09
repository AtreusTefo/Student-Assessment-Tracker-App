namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Explicit join entity for the many-to-many relationship between
    /// <see cref="Teacher"/> and <see cref="Student"/>.
    /// A student in a real school is taught by one teacher per subject, so a single
    /// student row can have multiple rows here — one for each subject teacher.
    /// </summary>
    public class TeacherStudent
    {
        /// <summary>FK → Teachers.Id</summary>
        public int TeacherId { get; set; }

        /// <summary>Navigation to the teacher side of the assignment.</summary>
        public Teacher Teacher { get; set; } = null!;

        /// <summary>FK → Students.Id</summary>
        public int StudentId { get; set; }

        /// <summary>Navigation to the student side of the assignment.</summary>
        public Student Student { get; set; } = null!;

        /// <summary>
        /// Denormalized copy of the teacher's SubjectId at assignment time.
        /// Together with StudentId this forms a unique index that enforces
        /// one-teacher-per-subject-per-student at the database level (Issue 1 fix).
        /// FK → Subjects.Id (RESTRICT).
        /// </summary>
        public int SubjectId { get; set; }

        /// <summary>Navigation to the subject of this assignment.</summary>
        public Subject Subject { get; set; } = null!;

        /// <summary>UTC timestamp recording when the assignment was created.</summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
