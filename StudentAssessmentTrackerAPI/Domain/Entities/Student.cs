namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Student domain entity - contains core business logic for student assessments
    /// </summary>
    public class Student
    {
        /// <summary>
        /// Unique identifier for the student
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Student's first name
        /// </summary>
        public string? FirstName { get; set; }
        /// <summary>
        /// Student's last name
        /// </summary>
        public string? LastName { get; set; }
        /// <summary>
        /// Student's email address
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// Student's phone number
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// Student's grade or class level
        /// </summary>
        public string? Grade { get; set; }
        /// <summary>
        /// Score for first assessment (0-20)
        /// </summary>
        public decimal Assessment1 { get; set; }
        /// <summary>
        /// Score for second assessment (0-20)
        /// </summary>
        public decimal Assessment2 { get; set; }
        /// <summary>
        /// Score for third assessment (0-20)
        /// </summary>
        public decimal Assessment3 { get; set; }
        /// <summary>
        /// Date when student record was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Date when student record was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Calculates the total score from all three assessments
        /// Domain Logic: Business rule enforced at entity level
        /// </summary>
        public decimal GetTotalScore() => Assessment1 + Assessment2 + Assessment3;

        /// <summary>
        /// Calculates the average score
        /// Domain Logic: Divides total by number of assessments
        /// </summary>
        public decimal GetAverageScore() => GetTotalScore() / 3;

        /// <summary>
        /// Calculates the percentage out of 60 (max possible score)
        /// Domain Logic: (Total / 60) * 100
        /// </summary>
        public decimal GetPercentage() => (GetTotalScore() / 60) * 100;

        /// <summary>
        /// Determines performance level based on percentage
        /// Domain Logic: Business rules for classification
        /// </summary>
        public string GetPerformanceLevel()
        {
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
