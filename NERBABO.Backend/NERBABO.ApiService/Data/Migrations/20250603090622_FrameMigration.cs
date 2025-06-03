using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NERBABO.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class FrameMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Frames",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Program = table.Column<string>(type: "varchar(150)", nullable: false),
                    Intervention = table.Column<string>(type: "varchar(55)", nullable: false),
                    InterventionType = table.Column<string>(type: "varchar(150)", nullable: false),
                    Operation = table.Column<string>(type: "varchar(150)", nullable: false),
                    OperationType = table.Column<string>(type: "varchar(150)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frames", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Frames_Operation_Program",
                table: "Frames",
                columns: new[] { "Operation", "Program" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Frames");
        }
    }
}
