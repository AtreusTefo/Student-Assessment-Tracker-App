namespace StudentAssessmentTracker.Application.DTOs
{
    /// <summary>Full student record returned by admin/teacher endpoints, including calculated score fields.</summary>
    public class StudentDto
    {
        /// <summary>Unique database identifier.</summary>
        public int Id { get; set; }
        /// <summary>System-generated unique student identifier (e.g. STU-00001).</summary>
        public string? StudentUniqueId { get; set; }
        /// <summary>National ID or passport number of the student.</summary>
        public string? IdPassportNo { get; set; }
        /// <summary>Student's first name.</summary>
        public string? FirstName { get; set; }
        /// <summary>Student's last name.</summary>
        public string? LastName { get; set; }
        /// <summary>Student's email address.</summary>
        public string? Email { get; set; }
        /// <summary>Student's phone number.</summary>
        public string? Phone { get; set; }
        /// <summary>Foreign key referencing the Grades lookup table.</summary>
        public int GradeId { get; set; }
        /// <summary>Resolved grade name (e.g. "Grade 10").</summary>
        public string? GradeName { get; set; }
        /// <summary>Teachers currently assigned to this student, each teaching a different subject.</summary>
        public IEnumerable<TeacherSummaryDto> Teachers { get; set; } = new List<TeacherSummaryDto>();
        /// <summary>Collection of assessments belonging to this student.</summary>
        public IEnumerable<StudentAssessmentDto> Assessments { get; set; } = new List<StudentAssessmentDto>();
        /// <summary>Sum of scores across all assessments.</summary>
        public decimal TotalScore { get; set; }
        /// <summary>Sum of max possible scores across all assessments.</summary>
        public decimal MaxPossible { get; set; }
        /// <summary>Average score per assessment.</summary>
        public decimal AverageScore { get; set; }
        /// <summary>Overall percentage score (TotalScore / MaxPossible * 100).</summary>
        public decimal Percentage { get; set; }
        /// <summary>Performance level label (e.g. "Excellent", "Pass", "Fail").</summary>
        public string? PerformanceLevel { get; set; }
        /// <summary>UTC timestamp when the record was created.</summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>UTC timestamp when the record was last updated.</summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>Payload for registering a new student.</summary>
    public class CreateStudentDto
    {
        /// <summary>National ID or passport number (must be unique).</summary>
        public string? IdPassportNo { get; set; }
        /// <summary>Student's first name.</summary>
        public string? FirstName { get; set; }
        /// <summary>Student's last name.</summary>
        public string? LastName { get; set; }
        /// <summary>Student's email address.</summary>
        public string? Email { get; set; }
        /// <summary>Student's phone number.</summary>
        public string? Phone { get; set; }
        /// <summary>Foreign key referencing the Grades lookup table.</summary>
        public int GradeId { get; set; }
        // TeacherId is intentionally absent — it is taken from the authenticated teacher's JWT claim
    }

    /// <summary>Payload for updating an existing student record (StudentUniqueId and TeacherId are immutable).</summary>
    public class UpdateStudentDto
    {
        /// <summary>National ID or passport number.</summary>
        public string? IdPassportNo { get; set; }
        /// <summary>Student's first name.</summary>
        public string? FirstName { get; set; }
        /// <summary>Student's last name.</summary>
        public string? LastName { get; set; }
        /// <summary>Student's email address.</summary>
        public string? Email { get; set; }
        /// <summary>Student's phone number.</summary>
        public string? Phone { get; set; }
        /// <summary>Foreign key referencing the Grades lookup table.</summary>
        public int GradeId { get; set; }
    }

    /// <summary>Safe public profile returned to the student after login/activation (no password field)</summary>
    public class StudentProfileDto
    {
        /// <summary>Unique database identifier.</summary>
        public int Id { get; set; }
        /// <summary>System-generated unique student identifier.</summary>
        public string? StudentUniqueId { get; set; }
        /// <summary>National ID or passport number.</summary>
        public string? IdPassportNo { get; set; }
        /// <summary>Student's first name.</summary>
        public string? FirstName { get; set; }
        /// <summary>Student's last name.</summary>
        public string? LastName { get; set; }
        /// <summary>Student's email address.</summary>
        public string? Email { get; set; }
        /// <summary>Student's phone number.</summary>
        public string? Phone { get; set; }
        /// <summary>Foreign key referencing the Grades lookup table.</summary>
        public int GradeId { get; set; }
        /// <summary>Resolved grade name.</summary>
        public string? GradeName { get; set; }
        /// <summary>Teachers currently assigned to this student, each teaching a different subject.</summary>
        public IEnumerable<TeacherSummaryDto> Teachers { get; set; } = new List<TeacherSummaryDto>();
        /// <summary>Collection of assessments belonging to this student.</summary>
        public IEnumerable<StudentAssessmentDto> Assessments { get; set; } = new List<StudentAssessmentDto>();
        /// <summary>Sum of scores across all assessments.</summary>
        public decimal TotalScore { get; set; }
        /// <summary>Sum of max possible scores across all assessments.</summary>
        public decimal MaxPossible { get; set; }
        /// <summary>Average score per assessment.</summary>
        public decimal AverageScore { get; set; }
        /// <summary>Overall percentage score.</summary>
        public decimal Percentage { get; set; }
        /// <summary>Performance level label.</summary>
        public string? PerformanceLevel { get; set; }
        /// <summary>UTC timestamp when the record was created.</summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>UTC timestamp when the record was last updated.</summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>DTO for student account activation — links StudentUniqueId + Email to a new password</summary>
    public class StudentActivateDto
    {
        /// <summary>System-generated unique student identifier used to verify identity.</summary>
        public string StudentUniqueId { get; set; } = string.Empty;
        /// <summary>Email address on record for the student.</summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>New password the student wants to set.</summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>Must match <see cref="Password"/>. Server-side check prevents direct API calls from bypassing frontend confirmation.</summary>
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>DTO for student login credentials</summary>
    public class StudentLoginDto
    {
        /// <summary>System-generated unique student identifier.</summary>
        public string StudentUniqueId { get; set; } = string.Empty;
        /// <summary>Student's password.</summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>DTO returned on successful student login or activation</summary>
    public class StudentLoginResponseDto
    {
        /// <summary>JWT bearer token for subsequent authenticated requests.</summary>
        public string Token { get; set; } = string.Empty;
        /// <summary>Public profile of the authenticated student.</summary>
        public StudentProfileDto Student { get; set; } = new();
    }
}
