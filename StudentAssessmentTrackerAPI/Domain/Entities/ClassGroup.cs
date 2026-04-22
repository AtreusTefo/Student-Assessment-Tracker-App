namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// A named class group that groups students by Grade and Subject under a specific teacher.
    /// Example: "Grade 10 Mathematics — Morning", owned by a Mathematics teacher.
    /// This enables a teacher to broadcast assessments to all enrolled students at once
    /// rather than adding them one by one.
    /// </summary>
    public class ClassGroup
    {
        /// <summary>Auto-incremented primary key.</summary>
        public int Id { get; set; }

        /// <summary>Human-readable name, e.g. "Grade 10 Maths – Period 2".</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>FK → Subjects.Id — the subject taught in this class.</summary>
        public int SubjectId { get; set; }

        /// <summary>Navigation property for the associated subject.</summary>
        public Subject? Subject { get; set; }

        /// <summary>FK → Grades.Id — the grade level of this class.</summary>
        public int GradeId { get; set; }

        /// <summary>Navigation property for the associated grade level.</summary>
        public Grade? Grade { get; set; }

        /// <summary>FK → Teachers.Id — the teacher who owns/created this class group.</summary>
        public int TeacherId { get; set; }

        /// <summary>Navigation property for the owning teacher.</summary>
        public Teacher? Teacher { get; set; }

        /// <summary>UTC timestamp when the class group was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp of the most recent update to this class group.</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Students enrolled in this class group (many-to-many via ClassGroupStudent).</summary>
        public ICollection<ClassGroupStudent> Enrollments { get; set; } = new List<ClassGroupStudent>();
    }
}
