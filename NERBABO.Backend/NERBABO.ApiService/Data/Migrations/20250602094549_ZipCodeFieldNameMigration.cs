using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NERBABO.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class ZipCodeFieldNameMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "People",
                newName: "ZipCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ZipCode",
                table: "People",
                newName: "PostalCode");
        }
    }
}
