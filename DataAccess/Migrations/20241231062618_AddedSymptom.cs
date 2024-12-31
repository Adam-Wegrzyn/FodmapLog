using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedSymptom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Symptom",
                table: "SymptomsLogs");

            migrationBuilder.DropColumn(
                name: "SymptomScale",
                table: "SymptomsLogs");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "SymptomsLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Symptom",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SymptomType = table.Column<int>(type: "int", nullable: false),
                    SymptomScale = table.Column<int>(type: "int", nullable: false),
                    SymptomsLogId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Symptom", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Symptom_SymptomsLogs_SymptomsLogId",
                        column: x => x.SymptomsLogId,
                        principalTable: "SymptomsLogs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Symptom_SymptomsLogId",
                table: "Symptom",
                column: "SymptomsLogId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Symptom");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "SymptomsLogs");

            migrationBuilder.AddColumn<int>(
                name: "Symptom",
                table: "SymptomsLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SymptomScale",
                table: "SymptomsLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
