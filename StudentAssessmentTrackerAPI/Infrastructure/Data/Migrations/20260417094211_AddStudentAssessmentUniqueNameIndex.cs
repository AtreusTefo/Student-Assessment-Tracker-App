using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAssessmentTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentAssessmentUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentAssessments_StudentId",
                table: "StudentAssessments");

            migrationBuilder.CreateIndex(
                name: "UX_StudentAssessments_StudentId_Name",
                table: "StudentAssessments",
                columns: new[] { "StudentId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_StudentAssessments_StudentId_Name",
                table: "StudentAssessments");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssessments_StudentId",
                table: "StudentAssessments",
                column: "StudentId");
        }
    }
}
