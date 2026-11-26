using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class LogTimeUtcAndSpokenSymptom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UtcInstant",
                table: "SymptomsLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SpokenName",
                table: "Symptoms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UtcInstant",
                table: "MealLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UtcInstant",
                table: "SymptomsLogs");

            migrationBuilder.DropColumn(
                name: "SpokenName",
                table: "Symptoms");

            migrationBuilder.DropColumn(
                name: "UtcInstant",
                table: "MealLogs");
        }
    }
}
