namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Student domain entity - contains core business logic for student assessments.
    /// Assessment scores are stored in the related StudentAssessments collection,
    /// not as columns on this row, allowing any number of named assessments with
    /// flexible max scores and optional due dates.
    /// </summary>
    public class Student
    {
        /// <summary>Unique identifier (auto-incremented PK)</summary>
        public int Id { get; set; }

        /// <summary>System-generated unique student ID (e.g., STU-A1B2C3D4), never changes</summary>
        public string? StudentUniqueId { get; set; }

        /// <summary>Student's national ID or passport number — unique across all students</summary>
        public string? IdPassportNo { get; set; }

        /// <summary>Student's first name</summary>
        public string? FirstName { get; set; }

        /// <summary>Student's last name</summary>
        public string? LastName { get; set; }

        /// <summary>Student's email address</summary>
        public string? Email { get; set; }

        /// <summary>Student's phone number</summary>
        public string? Phone { get; set; }

        /// <summary>Password set during account activation — null until the student activates their account</summary>
        public string? Password { get; set; }

        /// <summary>FK → Grades.Id — enforces a controlled grade level (Grade 7–12)</summary>
        public int GradeId { get; set; }

        /// <summary>Navigation property to the Grade lookup entry</summary>
        public Grade? GradeNavigation { get; set; }

        /// <summary>
        /// Many-to-many: the teachers currently assigned to instruct this student.
        /// A Grade-10 student can have a Maths teacher, an English teacher, etc. —
        /// each represented by one <see cref="TeacherStudent"/> row.
        /// </summary>
        public ICollection<TeacherStudent> TeacherAssignments { get; set; } = new List<TeacherStudent>();

        /// <summary>Collection of this student's individual assessments</summary>
        public ICollection<StudentAssessment> Assessments { get; set; } = new List<StudentAssessment>();

        /// <summary>Record creation timestamp</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Record last-updated timestamp</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Sum of all assessment scores</summary>
        public decimal GetTotalScore() => Assessments.Sum(a => a.Score);

        /// <summary>Sum of all assessment max scores (the total possible marks)</summary>
        public decimal GetMaxPossible() => Assessments.Sum(a => a.MaxScore);

        /// <summary>
        /// Percentage based on actual max possible — not a hardcoded value.
        /// Returns 0 when no assessments exist.
        /// </summary>
        public decimal GetPercentage()
        {
            var max = GetMaxPossible();
            return max == 0 ? 0 : Math.Round((GetTotalScore() / max) * 100, 2);
        }

        /// <summary>
        /// Average score-as-percentage across all assessments.
        /// Returns 0 when no assessments exist.
        /// </summary>
        public decimal GetAverageScore()
        {
            if (!Assessments.Any()) return 0;
            return Math.Round(Assessments.Average(a => a.MaxScore == 0 ? 0 : (a.Score / a.MaxScore) * 100), 2);
        }

        /// <summary>Performance classification based on overall percentage</summary>
        public string GetPerformanceLevel()
        {
            if (!Assessments.Any()) return "No Assessments";
            var percentage = GetPercentage();
            return percentage switch
            {
                < 50 => "Needs Support",
                <= 55 => "Satisfactory",
                <= 75 => "Good",
                _ => "Excellent"
            };
        }
    }
}
