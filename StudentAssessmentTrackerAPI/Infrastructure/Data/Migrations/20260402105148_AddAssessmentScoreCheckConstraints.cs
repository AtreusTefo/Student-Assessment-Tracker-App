using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAssessmentTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentScoreCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_StudentAssessments_MaxScore_Positive",
                table: "StudentAssessments",
                sql: "[MaxScore] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StudentAssessments_Score_LteMaxScore",
                table: "StudentAssessments",
                sql: "[Score] <= [MaxScore]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StudentAssessments_Score_NonNegative",
                table: "StudentAssessments",
                sql: "[Score] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StudentAssessments_MaxScore_Positive",
                table: "StudentAssessments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StudentAssessments_Score_LteMaxScore",
                table: "StudentAssessments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StudentAssessments_Score_NonNegative",
                table: "StudentAssessments");
        }
    }
}
