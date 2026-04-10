namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Admin account entity. Admins have elevated access: they can view and manage
    /// all teachers, all students, and all audit logs across the entire system.
    /// Admins are registered separately from teachers and students and receive a
    /// JWT with role claim "Admin".
    /// </summary>
    public class Admin
    {
        /// <summary>Auto-incremented primary key.</summary>
        public int Id { get; set; }

        /// <summary>Admin's first name.</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>Admin's last name.</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>Admin's unique email address — used for login.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>BCrypt-hashed password.</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>UTC timestamp when the account was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp of the last profile update.</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Returns the admin's full display name.</summary>
        public string GetFullName() => $"{FirstName} {LastName}";
    }
}
