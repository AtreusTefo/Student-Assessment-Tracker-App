using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAssessmentTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectIdToTeacherStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeacherStudents_StudentId",
                table: "TeacherStudents");

            // Step 1: Add the column as nullable so existing rows don't immediately
            // violate the FK before we can backfill.
            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "TeacherStudents",
                type: "int",
                nullable: true,
                defaultValue: null);

            // Step 2: Backfill from the owning teacher's SubjectId so every existing
            // assignment row reflects the correct subject.
            migrationBuilder.Sql(
                @"UPDATE ts
                  SET ts.SubjectId = t.SubjectId
                  FROM TeacherStudents ts
                  INNER JOIN Teachers t ON ts.TeacherId = t.Id");

            // Step 3: Now that every row has a valid SubjectId, make it non-nullable.
            migrationBuilder.AlterColumn<int>(
                name: "SubjectId",
                table: "TeacherStudents",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherStudents_SubjectId",
                table: "TeacherStudents",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "UX_TeacherStudents_StudentId_SubjectId",
                table: "TeacherStudents",
                columns: new[] { "StudentId", "SubjectId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherStudents_Subjects_SubjectId",
                table: "TeacherStudents",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherStudents_Subjects_SubjectId",
                table: "TeacherStudents");

            migrationBuilder.DropIndex(
                name: "IX_TeacherStudents_SubjectId",
                table: "TeacherStudents");

            migrationBuilder.DropIndex(
                name: "UX_TeacherStudents_StudentId_SubjectId",
                table: "TeacherStudents");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "TeacherStudents");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherStudents_StudentId",
                table: "TeacherStudents",
                column: "StudentId");
        }
    }
}
