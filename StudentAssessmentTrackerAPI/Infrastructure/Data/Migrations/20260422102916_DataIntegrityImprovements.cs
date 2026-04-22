using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAssessmentTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DataIntegrityImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_AssessmentSubmissions_ContentType_Allowed",
                table: "AssessmentSubmissions",
                sql: "[ContentType] IN ('application/pdf','application/msword','application/vnd.openxmlformats-officedocument.wordprocessingml.document','image/jpeg','image/png')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AssessmentSubmissions_FileName_NotEmpty",
                table: "AssessmentSubmissions",
                sql: "LEN(LTRIM(RTRIM([FileName]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AssessmentSubmissions_FileSize_Positive",
                table: "AssessmentSubmissions",
                sql: "[FileSize] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AssessmentSubmissions_ContentType_Allowed",
                table: "AssessmentSubmissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AssessmentSubmissions_FileName_NotEmpty",
                table: "AssessmentSubmissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AssessmentSubmissions_FileSize_Positive",
                table: "AssessmentSubmissions");
        }
    }
}
