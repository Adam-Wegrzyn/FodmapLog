using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedTotalMacros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "TotalCarbohydrates",
                table: "MealLogs",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "TotalFat",
                table: "MealLogs",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "TotalKcal",
                table: "MealLogs",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "TotalProteins",
                table: "MealLogs",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalCarbohydrates",
                table: "MealLogs");

            migrationBuilder.DropColumn(
                name: "TotalFat",
                table: "MealLogs");

            migrationBuilder.DropColumn(
                name: "TotalKcal",
                table: "MealLogs");

            migrationBuilder.DropColumn(
                name: "TotalProteins",
                table: "MealLogs");
        }
    }
}
