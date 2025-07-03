using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NERBABO.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTaxPercValTypeMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "ValuePercent",
                table: "Taxes",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ValuePercent",
                table: "Taxes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");
        }
    }
}
