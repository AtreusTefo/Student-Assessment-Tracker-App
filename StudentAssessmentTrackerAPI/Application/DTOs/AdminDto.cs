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

    // ── Bulk Import DTOs ──────────────────────────────────────────────────────

    /// <summary>A single row for bulk student import (grade identified by display name).</summary>
    public class BulkImportStudentRowDto
    {
        /// <summary>National ID or passport number — must be unique across all users.</summary>
        public string? IdPassportNo { get; set; }
        /// <summary>Student's first name.</summary>
        public string? FirstName { get; set; }
        /// <summary>Student's last name.</summary>
        public string? LastName { get; set; }
        /// <summary>Student's email address — must be unique across all users.</summary>
        public string? Email { get; set; }
        /// <summary>Student's phone number (exactly 8 digits).</summary>
        public string? Phone { get; set; }
        /// <summary>Grade display name (e.g. "Grade 10") or level number (e.g. "10"). Resolved server-side.</summary>
        public string? GradeName { get; set; }
    }

    /// <summary>A single row for bulk teacher import (subject identified by display name).</summary>
    public class BulkImportTeacherRowDto
    {
        /// <summary>National ID or passport number — must be unique across all users.</summary>
        public string? IdPassportNo { get; set; }
        /// <summary>Teacher's first name.</summary>
        public string? FirstName { get; set; }
        /// <summary>Teacher's last name.</summary>
        public string? LastName { get; set; }
        /// <summary>Teacher's email address — must be unique across all users.</summary>
        public string? Email { get; set; }
        /// <summary>Teacher's phone number (exactly 8 digits).</summary>
        public string? Phone { get; set; }
        /// <summary>Subject display name (e.g. "Mathematics"). Matched case-insensitively against the Subjects lookup.</summary>
        public string? SubjectName { get; set; }
    }

    /// <summary>Per-row outcome from a bulk import operation.</summary>
    public class BulkImportRowResultDto
    {
        /// <summary>1-based row number within the submitted list.</summary>
        public int Row { get; set; }
        /// <summary>Whether this row was successfully imported.</summary>
        public bool Success { get; set; }
        /// <summary>On success: email or "UniqueId (email)" for students. On failure: the submitted email or IdPassportNo.</summary>
        public string? Identifier { get; set; }
        /// <summary>Validation or conflict error message; null on success.</summary>
        public string? Error { get; set; }
    }

    /// <summary>Aggregate result of a bulk import operation.</summary>
    public class BulkImportResultDto
    {
        /// <summary>Total rows submitted in the request.</summary>
        public int TotalRows { get; set; }
        /// <summary>Number of rows that were successfully imported.</summary>
        public int SuccessCount { get; set; }
        /// <summary>Number of rows that failed validation or encountered conflicts.</summary>
        public int FailureCount { get; set; }
        /// <summary>Per-row results in submission order.</summary>
        public List<BulkImportRowResultDto> Results { get; set; } = new();
    }
}
