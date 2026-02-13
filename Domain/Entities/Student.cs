namespace StudentAssessmentTracker.Domain.Entities
{
    /// <summary>
    /// Student domain entity - contains core business logic for student assessments
    /// </summary>
    public class Student
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Grade { get; set; }
        public decimal Assessment1 { get; set; }
        public decimal Assessment2 { get; set; }
        public decimal Assessment3 { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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
