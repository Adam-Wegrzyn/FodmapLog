using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MealLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nutriments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fat = table.Column<float>(type: "real", nullable: true),
                    FatServing = table.Column<float>(type: "real", nullable: true),
                    Fat100g = table.Column<float>(type: "real", nullable: true),
                    FatUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sugars = table.Column<float>(type: "real", nullable: true),
                    SugarsServing = table.Column<float>(type: "real", nullable: true),
                    Sugars100g = table.Column<float>(type: "real", nullable: true),
                    SugarsUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Carbohydrates = table.Column<float>(type: "real", nullable: true),
                    Carbohydrates100g = table.Column<float>(type: "real", nullable: true),
                    CarbohydratesUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnergyKcal100g = table.Column<float>(type: "real", nullable: true),
                    EnergyKcalServing = table.Column<float>(type: "real", nullable: true),
                    EnergyKcalUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Proteins = table.Column<float>(type: "real", nullable: true),
                    ProteinsServing = table.Column<float>(type: "real", nullable: true),
                    Proteins100g = table.Column<float>(type: "real", nullable: true),
                    ProteinsUnit = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nutriments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SymptomsLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symptom = table.Column<int>(type: "int", nullable: false),
                    SymptomScale = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SymptomsLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdExternal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductQuantity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductQuantityUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServingQuantity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServingQuantityUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NutrimentsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Nutriments_NutrimentsId",
                        column: x => x.NutrimentsId,
                        principalTable: "Nutriments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductQuantity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<float>(type: "real", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    MealLogId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductQuantity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductQuantity_MealLogs_MealLogId",
                        column: x => x.MealLogId,
                        principalTable: "MealLogs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductQuantity_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuantity_MealLogId",
                table: "ProductQuantity",
                column: "MealLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuantity_ProductId",
                table: "ProductQuantity",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_NutrimentsId",
                table: "Products",
                column: "NutrimentsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductQuantity");

            migrationBuilder.DropTable(
                name: "SymptomsLogs");

            migrationBuilder.DropTable(
                name: "MealLogs");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Nutriments");
        }
    }
}
