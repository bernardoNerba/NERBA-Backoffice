using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NERBABO.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class TaxChangesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "boolean",
                table: "Taxes",
                newName: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Taxes",
                newName: "boolean");
        }
    }
}
