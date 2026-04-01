namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>
    /// DTO for teacher data returned from the API
    /// </summary>
    public class TeacherResponseDto
    {
        /// <summary>Gets or sets the teacher's unique identifier</summary>
        public int TeacherId { get; set; }

        /// <summary>Gets or sets the ID/Passport number</summary>
        public string IdPassportNo { get; set; } = string.Empty;

        /// <summary>Gets or sets the first name</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>Gets or sets the last name</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>Gets or sets the email address</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the phone number</summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>Gets or sets the subject identifier</summary>
        public int SubjectId { get; set; }

        /// <summary>Gets or sets the subject display name</summary>
        public string SubjectName { get; set; } = string.Empty;

        /// <summary>Gets or sets the enrollment date</summary>
        public DateTime EnrollmentDate { get; set; }

        /// <summary>Gets or sets the date the record was created</summary>
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// DTO for creating or registering a new teacher
    /// </summary>
    public class TeacherRegisterDto
    {
        /// <summary>Gets or sets the ID/Passport number</summary>
        public string IdPassportNo { get; set; } = string.Empty;

        /// <summary>Gets or sets the first name</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>Gets or sets the last name</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>Gets or sets the email address</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the phone number</summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>Gets or sets the subject identifier</summary>
        public int SubjectId { get; set; }

        /// <summary>Gets or sets the password</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Gets or sets the enrollment date</summary>
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO for updating an existing teacher's profile.
    /// Password changes are not supported via this DTO — a dedicated change-password
    /// flow requiring the current password must be used instead.
    /// </summary>
    public class TeacherUpdateDto
    {
        /// <summary>Gets or sets the ID/Passport number</summary>
        public string IdPassportNo { get; set; } = string.Empty;

        /// <summary>Gets or sets the first name</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>Gets or sets the last name</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>Gets or sets the email address</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the phone number</summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>Gets or sets the subject identifier</summary>
        public int SubjectId { get; set; }

        /// <summary>Gets or sets the enrollment date</summary>
        public DateTime EnrollmentDate { get; set; }
    }

    /// <summary>
    /// DTO for teacher login credentials
    /// </summary>
    public class TeacherLoginDto
    {
        /// <summary>Gets or sets the email address</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the password</summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO returned upon successful login
    /// </summary>
    public class TeacherLoginResponseDto
    {
        /// <summary>Gets or sets the authentication token (demo value)</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>Gets or sets the authenticated teacher's data</summary>
        public TeacherResponseDto Teacher { get; set; } = new();
    }

    /// <summary>
    /// Minimal teacher reference embedded inside student response DTOs.
    /// Shows which teacher (by name and subject) is assigned to a given student.
    /// </summary>
    public class TeacherSummaryDto
    {
        /// <summary>Teacher's primary key.</summary>
        public int TeacherId { get; set; }
        /// <summary>Teacher's full name (FirstName + LastName).</summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>Subject this teacher is responsible for.</summary>
        public string SubjectName { get; set; } = string.Empty;
    }
}
