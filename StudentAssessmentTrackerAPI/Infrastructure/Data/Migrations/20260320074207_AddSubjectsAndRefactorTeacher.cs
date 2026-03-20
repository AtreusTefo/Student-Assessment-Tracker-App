using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudentAssessmentTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectsAndRefactorTeacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create Subjects lookup table
            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Name",
                table: "Subjects",
                column: "Name",
                unique: true);

            // 2. Seed subject data
            migrationBuilder.InsertData(
                table: "Subjects",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Accounting" },
                    { 2, "Art" },
                    { 3, "Business Studies" },
                    { 4, "English" },
                    { 5, "Geography" },
                    { 6, "History" },
                    { 7, "ICT" },
                    { 8, "Mathematics" },
                    { 9, "Multimedia" },
                    { 10, "Music" },
                    { 11, "Physical Education" },
                    { 12, "Science" }
                });

            // 3. Add SubjectId as nullable first so existing rows don't violate NOT NULL
            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "Teachers",
                type: "int",
                nullable: true,
                defaultValue: null);

            // 4. Migrate existing free-text Subject values to the nearest matching SubjectId
            //    Case-insensitive best-effort match; unmatched rows default to ICT (Id=7)
            migrationBuilder.Sql(@"
                UPDATE Teachers SET SubjectId = CASE
                    WHEN LOWER(Subject) LIKE '%accounting%'   THEN 1
                    WHEN LOWER(Subject) LIKE '%art%'          THEN 2
                    WHEN LOWER(Subject) LIKE '%business%'     THEN 3
                    WHEN LOWER(Subject) LIKE '%english%'      THEN 4
                    WHEN LOWER(Subject) LIKE '%geography%'    THEN 5
                    WHEN LOWER(Subject) LIKE '%history%'      THEN 6
                    WHEN LOWER(Subject) LIKE '%ict%'          THEN 7
                    WHEN LOWER(Subject) LIKE '%math%'         THEN 8
                    WHEN LOWER(Subject) LIKE '%multimedia%'   THEN 9
                    WHEN LOWER(Subject) LIKE '%music%'        THEN 10
                    WHEN LOWER(Subject) LIKE '%physical%'     THEN 11
                    WHEN LOWER(Subject) LIKE '%p.e%'          THEN 11
                    WHEN LOWER(Subject) LIKE '%science%'      THEN 12
                    ELSE 7  -- default to ICT for any unrecognised value
                END
            ");

            // 5. Make SubjectId NOT NULL now that all rows have a valid value
            migrationBuilder.AlterColumn<int>(
                name: "SubjectId",
                table: "Teachers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 6. Add FK constraint
            migrationBuilder.CreateIndex(
                name: "IX_Teachers_SubjectId",
                table: "Teachers",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Subjects_SubjectId",
                table: "Teachers",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 7. Drop old free-text Subject column — data safely migrated above
            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Teachers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Subjects_SubjectId",
                table: "Teachers");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_SubjectId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Teachers");

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Teachers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
