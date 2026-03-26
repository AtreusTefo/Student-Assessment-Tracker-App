using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAssessmentTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdPassportNoToTeacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdPassportNo",
                table: "Teachers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Backfill existing rows with unique placeholder values so the unique index can be created
            migrationBuilder.Sql(
                "UPDATE Teachers SET IdPassportNo = 'T' + RIGHT('00000000' + CAST(Id AS VARCHAR(8)), 8) WHERE IdPassportNo = ''");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_IdPassportNo",
                table: "Teachers",
                column: "IdPassportNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_IdPassportNo",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "IdPassportNo",
                table: "Teachers");
        }
    }
}
