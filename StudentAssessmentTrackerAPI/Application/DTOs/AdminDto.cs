namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>Admin profile returned after login and for management endpoints.</summary>
    public class AdminDto
    {
        /// <summary>Primary key.</summary>
        public int Id { get; set; }
        /// <summary>Admin's first name.</summary>
        public string FirstName { get; set; } = string.Empty;
        /// <summary>Admin's last name.</summary>
        public string LastName { get; set; } = string.Empty;
        /// <summary>Admin's email address.</summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>UTC timestamp of account creation.</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Payload for creating a new admin account.</summary>
    public class CreateAdminDto
    {
        /// <summary>Admin's first name.</summary>
        public string? FirstName { get; set; }
        /// <summary>Admin's last name.</summary>
        public string? LastName { get; set; }
        /// <summary>Unique email — used for login.</summary>
        public string? Email { get; set; }
        /// <summary>Plain-text password (hashed before storage).</summary>
        public string? Password { get; set; }
    }

    /// <summary>Payload for admin login.</summary>
    public class AdminLoginDto
    {
        /// <summary>Admin email address.</summary>
        public string? Email { get; set; }
        /// <summary>Admin password.</summary>
        public string? Password { get; set; }
    }

    /// <summary>Response returned on successful admin login.</summary>
    public class AdminLoginResponseDto
    {
        /// <summary>Signed JWT for the "Admin" role.</summary>
        public string Token { get; set; } = string.Empty;
        /// <summary>Admin profile payload.</summary>
        public AdminDto Admin { get; set; } = new();
    }

    /// <summary>Payload for changing an admin's password.</summary>
    public class ChangeAdminPasswordDto
    {
        /// <summary>The admin's current password (required for verification).</summary>
        public string? CurrentPassword { get; set; }
        /// <summary>The new password to set (min 6 characters).</summary>
        public string? NewPassword { get; set; }
        /// <summary>Must match NewPassword exactly.</summary>
        public string? ConfirmNewPassword { get; set; }
    }
}
