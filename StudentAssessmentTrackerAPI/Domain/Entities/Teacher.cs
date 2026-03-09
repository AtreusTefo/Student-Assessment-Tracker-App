namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Teacher domain entity representing an instructor in the system
    /// </summary>
    public class Teacher
    {
        /// <summary>Gets or sets the unique identifier</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the teacher's first name</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>Gets or sets the teacher's last name</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>Gets or sets the teacher's email address</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the teacher's phone number</summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>Gets or sets the subject the teacher instructs</summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>Gets or sets the hashed or stored password</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Gets or sets the enrollment date</summary>
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        /// <summary>Gets or sets the creation date</summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>Gets the teacher's full name</summary>
        public string GetFullName() => $"{FirstName} {LastName}";
    }
}
