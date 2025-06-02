using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NERBABO.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class PersonMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PersonId",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "varchar(100)", nullable: false),
                    LastName = table.Column<string>(type: "varchar(100)", nullable: false),
                    NIF = table.Column<string>(type: "char(9)", nullable: false),
                    IdentificationNumber = table.Column<string>(type: "varchar(10)", nullable: true),
                    IdentificationValidationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IdentificationType = table.Column<string>(type: "varchar(25)", nullable: false),
                    NISS = table.Column<string>(type: "varchar(11)", nullable: true),
                    IBAN = table.Column<string>(type: "varchar(25)", nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "varchar(8)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(9)", nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", nullable: true),
                    Naturality = table.Column<string>(type: "varchar(100)", nullable: true),
                    Nationality = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "varchar(25)", nullable: false),
                    Habilitation = table.Column<string>(type: "varchar(25)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PersonId",
                table: "Users",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_NIF",
                table: "People",
                column: "NIF",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_People_PersonId",
                table: "Users",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_People_PersonId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropIndex(
                name: "IX_Users_PersonId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Users");
        }
    }
}
