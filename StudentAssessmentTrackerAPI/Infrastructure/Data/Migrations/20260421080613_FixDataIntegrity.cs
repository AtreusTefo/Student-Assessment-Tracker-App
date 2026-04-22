using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAssessmentTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixDataIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassGroups_Teachers_TeacherId",
                table: "ClassGroups");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Teachers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Teachers_Email_Lowercase",
                table: "Teachers",
                sql: "[Email] = LOWER([Email])");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Teachers_Phone_Format",
                table: "Teachers",
                sql: "[Phone] NOT LIKE '%[^0-9]%' AND LEN([Phone]) = 8");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Students_Email_Lowercase",
                table: "Students",
                sql: "[Email] = LOWER([Email])");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Students_Phone_Format",
                table: "Students",
                sql: "[Phone] NOT LIKE '%[^0-9]%' AND LEN([Phone]) = 8");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AuditLogs_ChangedByRole",
                table: "AuditLogs",
                sql: "[ChangedByRole] IS NULL OR [ChangedByRole] IN ('Teacher', 'Student', 'Admin')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Admins_Email_Lowercase",
                table: "Admins",
                sql: "[Email] = LOWER([Email])");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassGroups_Teachers_TeacherId",
                table: "ClassGroups",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassGroups_Teachers_TeacherId",
                table: "ClassGroups");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Teachers_Email_Lowercase",
                table: "Teachers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Teachers_Phone_Format",
                table: "Teachers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Students_Email_Lowercase",
                table: "Students");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Students_Phone_Format",
                table: "Students");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AuditLogs_ChangedByRole",
                table: "AuditLogs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Admins_Email_Lowercase",
                table: "Admins");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Teachers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassGroups_Teachers_TeacherId",
                table: "ClassGroups",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
