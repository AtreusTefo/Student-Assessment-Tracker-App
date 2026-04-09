using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAssessmentTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixTeacherFKDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherStudents_Teachers_TeacherId",
                table: "TeacherStudents");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherStudents_Teachers_TeacherId",
                table: "TeacherStudents",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherStudents_Teachers_TeacherId",
                table: "TeacherStudents");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherStudents_Teachers_TeacherId",
                table: "TeacherStudents",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
