using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NERBABO.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class GlobalMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IvaTaxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false),
                    ValuePercent = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IvaTaxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneralInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Designation = table.Column<string>(type: "varchar(255)", nullable: false),
                    IvaId = table.Column<int>(type: "integer", nullable: true),
                    Site = table.Column<string>(type: "text", nullable: false),
                    HourValueTeacher = table.Column<float>(type: "decimal", nullable: false),
                    HourValueAlimentation = table.Column<float>(type: "decimal", nullable: false),
                    BankEntity = table.Column<string>(type: "varchar(50)", nullable: false),
                    Iban = table.Column<string>(type: "char(25)", nullable: false),
                    Nipc = table.Column<string>(type: "char(9)", nullable: false),
                    LogoFinancing = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneralInfo_IvaTaxes_IvaId",
                        column: x => x.IvaId,
                        principalTable: "IvaTaxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralInfo_IvaId",
                table: "GeneralInfo",
                column: "IvaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralInfo");

            migrationBuilder.DropTable(
                name: "IvaTaxes");
        }
    }
}
