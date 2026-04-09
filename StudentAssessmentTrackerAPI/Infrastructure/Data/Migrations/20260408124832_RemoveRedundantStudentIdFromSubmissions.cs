using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAssessmentTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantStudentIdFromSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSubmissions_Students_StudentId",
                table: "AssessmentSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentSubmissions_StudentId",
                table: "AssessmentSubmissions");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "AssessmentSubmissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "AssessmentSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentSubmissions_StudentId",
                table: "AssessmentSubmissions",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSubmissions_Students_StudentId",
                table: "AssessmentSubmissions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }
    }
}
