using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Carbohydrates",
                table: "Nutriments");

            migrationBuilder.RenameColumn(
                name: "Sugars",
                table: "Nutriments",
                newName: "FiberServing");

            migrationBuilder.RenameColumn(
                name: "Proteins",
                table: "Nutriments",
                newName: "Fiber100g");

            migrationBuilder.RenameColumn(
                name: "Fat",
                table: "Nutriments",
                newName: "CarbohydratesServing");

            migrationBuilder.AddColumn<string>(
                name: "FiberUnit",
                table: "Nutriments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FiberUnit",
                table: "Nutriments");

            migrationBuilder.RenameColumn(
                name: "FiberServing",
                table: "Nutriments",
                newName: "Sugars");

            migrationBuilder.RenameColumn(
                name: "Fiber100g",
                table: "Nutriments",
                newName: "Proteins");

            migrationBuilder.RenameColumn(
                name: "CarbohydratesServing",
                table: "Nutriments",
                newName: "Fat");

            migrationBuilder.AddColumn<float>(
                name: "Carbohydrates",
                table: "Nutriments",
                type: "real",
                nullable: true);
        }
    }
}
