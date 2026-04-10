namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>Class group details returned by API endpoints.</summary>
    public class ClassGroupDto
    {
        /// <summary>Primary key.</summary>
        public int Id { get; set; }
        /// <summary>Display name of the class group.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>FK to Subjects.</summary>
        public int SubjectId { get; set; }
        /// <summary>Resolved subject name.</summary>
        public string SubjectName { get; set; } = string.Empty;
        /// <summary>FK to Grades.</summary>
        public int GradeId { get; set; }
        /// <summary>Resolved grade name, e.g. "Grade 10".</summary>
        public string GradeName { get; set; } = string.Empty;
        /// <summary>FK to Teachers — owning teacher.</summary>
        public int TeacherId { get; set; }
        /// <summary>UTC creation timestamp.</summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>Number of students enrolled in this group.</summary>
        public int StudentCount { get; set; }
        /// <summary>Students enrolled in this class group.</summary>
        public IEnumerable<ClassGroupMemberDto> Students { get; set; } = new List<ClassGroupMemberDto>();
    }

    /// <summary>Lightweight student record embedded in a ClassGroupDto.</summary>
    public class ClassGroupMemberDto
    {
        /// <summary>Student primary key.</summary>
        public int StudentId { get; set; }
        /// <summary>System-generated unique student identifier.</summary>
        public string StudentUniqueId { get; set; } = string.Empty;
        /// <summary>Student's full name.</summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>UTC timestamp when the student was enrolled in the group.</summary>
        public DateTime EnrolledAt { get; set; }
    }

    /// <summary>Payload for creating a new class group.</summary>
    public class CreateClassGroupDto
    {
        /// <summary>Display name for the group.</summary>
        public string? Name { get; set; }
        /// <summary>FK to Subjects lookup.</summary>
        public int SubjectId { get; set; }
        /// <summary>FK to Grades lookup.</summary>
        public int GradeId { get; set; }
    }

    /// <summary>Payload for updating a class group name.</summary>
    public class UpdateClassGroupDto
    {
        /// <summary>New display name for the group.</summary>
        public string? Name { get; set; }
    }

    /// <summary>Payload for enrolling or removing a student in a class group.</summary>
    public class ClassGroupEnrollDto
    {
        /// <summary>The student's primary key to enroll or remove.</summary>
        public int StudentId { get; set; }
    }
}
