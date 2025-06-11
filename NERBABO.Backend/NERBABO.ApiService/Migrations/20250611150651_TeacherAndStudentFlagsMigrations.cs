using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NERBABO.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class TeacherAndStudentFlagsMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Teachers");

            migrationBuilder.AddColumn<bool>(
                name: "IsLecturingCQ",
                table: "Teachers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLecturingFM",
                table: "Teachers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnrolledInCQ",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnrolledInFM",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLecturingCQ",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "IsLecturingFM",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "EnrolledInCQ",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EnrolledInFM",
                table: "Students");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Teachers",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }
    }
}
