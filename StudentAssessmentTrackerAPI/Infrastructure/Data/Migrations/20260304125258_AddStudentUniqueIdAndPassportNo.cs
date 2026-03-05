using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAssessmentTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentUniqueIdAndPassportNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdPassportNo",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StudentUniqueId",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentUniqueId",
                table: "Students",
                column: "StudentUniqueId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_StudentUniqueId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IdPassportNo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StudentUniqueId",
                table: "Students");
        }
    }
}
