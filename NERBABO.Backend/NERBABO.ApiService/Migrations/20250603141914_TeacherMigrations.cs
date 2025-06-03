using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NERBABO.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class TeacherMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IvaRegimeId = table.Column<int>(type: "integer", nullable: false),
                    IrsRegimeId = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<long>(type: "bigint", nullable: false),
                    Ccp = table.Column<string>(type: "varchar(55)", nullable: false),
                    Competences = table.Column<string>(type: "text", nullable: false, defaultValue: "N/A"),
                    AvarageRating = table.Column<float>(type: "float", nullable: false, defaultValue: 0f),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teachers_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Teachers_Taxes_IrsRegimeId",
                        column: x => x.IrsRegimeId,
                        principalTable: "Taxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Teachers_Taxes_IvaRegimeId",
                        column: x => x.IvaRegimeId,
                        principalTable: "Taxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_IrsRegimeId",
                table: "Teachers",
                column: "IrsRegimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_IvaRegimeId",
                table: "Teachers",
                column: "IvaRegimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_PersonId",
                table: "Teachers",
                column: "PersonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Teachers");
        }
    }
}
