using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudentAssessmentTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGradesAndAssessmentsRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Assessment1",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Assessment2",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Assessment3",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Students");

            migrationBuilder.AddColumn<int>(
                name: "GradeId",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaxScore = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAssessments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Grades",
                columns: new[] { "Id", "Level", "Name" },
                values: new object[,]
                {
                    { 1, 7, "Grade 7" },
                    { 2, 8, "Grade 8" },
                    { 3, 9, "Grade 9" },
                    { 4, 10, "Grade 10" },
                    { 5, 11, "Grade 11" },
                    { 6, 12, "Grade 12" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_GradeId",
                table: "Students",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_IdPassportNo",
                table: "Students",
                column: "IdPassportNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_TeacherId",
                table: "Students",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_Level",
                table: "Grades",
                column: "Level",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_Name",
                table: "Grades",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssessments_StudentId",
                table: "StudentAssessments",
                column: "StudentId");

            // ── Data migration ───────────────────────────────────────────────
            // Assign Grade 7 (Id=1) to any existing students that have no grade yet
            migrationBuilder.Sql("UPDATE Students SET GradeId = 1 WHERE GradeId = 0");

            // Assign the first available teacher to any existing orphaned student rows.
            // If no teachers exist the Students table should be empty, so this is a no-op.
            migrationBuilder.Sql(@"
                DECLARE @firstTeacherId INT = (SELECT TOP 1 Id FROM Teachers ORDER BY Id);
                IF @firstTeacherId IS NOT NULL AND EXISTS (SELECT 1 FROM Students WHERE TeacherId = 0)
                    UPDATE Students SET TeacherId = @firstTeacherId WHERE TeacherId = 0;
            ");
            // ────────────────────────────────────────────────────────────────

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Grades_GradeId",
                table: "Students",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Teachers_TeacherId",
                table: "Students",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Grades_GradeId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Teachers_TeacherId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "StudentAssessments");

            migrationBuilder.DropIndex(
                name: "IX_Students_GradeId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_IdPassportNo",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_TeacherId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "GradeId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Students");

            migrationBuilder.AddColumn<decimal>(
                name: "Assessment1",
                table: "Students",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Assessment2",
                table: "Students",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Assessment3",
                table: "Students",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "Students",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
