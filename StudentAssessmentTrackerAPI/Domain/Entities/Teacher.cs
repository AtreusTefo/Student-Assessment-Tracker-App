namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Teacher domain entity representing an instructor in the system
    /// </summary>
    public class Teacher
    {
        /// <summary>Gets or sets the unique identifier</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the teacher's ID/Passport number</summary>
        public string IdPassportNo { get; set; } = string.Empty;

        /// <summary>Gets or sets the teacher's first name</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>Gets or sets the teacher's last name</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>Gets or sets the teacher's email address</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the teacher's phone number</summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>Gets or sets the subject FK</summary>
        public int SubjectId { get; set; }

        /// <summary>Navigation property to the Subject lookup</summary>
        public Subject? SubjectNavigation { get; set; }

        /// <summary>Gets or sets the hashed or stored password</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Gets or sets the enrollment date</summary>
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        /// <summary>Gets or sets the creation date</summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>Gets or sets the last-updated timestamp (UTC).</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Many-to-many: the students this teacher is currently assigned to instruct.
        /// Each entry in this collection represents one student–teacher pairing.
        /// </summary>
        public ICollection<TeacherStudent> StudentAssignments { get; set; } = new List<TeacherStudent>();

        /// <summary>Gets the teacher's full name</summary>
        public string GetFullName() => $"{FirstName} {LastName}";
    }
}
