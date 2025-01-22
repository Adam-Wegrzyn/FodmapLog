using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateDbSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductQuantity_MealLogs_MealLogId",
                table: "ProductQuantity");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductQuantity_Products_ProductId",
                table: "ProductQuantity");

            migrationBuilder.DropForeignKey(
                name: "FK_Symptom_SymptomsLogs_SymptomsLogId",
                table: "Symptom");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Symptom",
                table: "Symptom");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductQuantity",
                table: "ProductQuantity");

            migrationBuilder.RenameTable(
                name: "Symptom",
                newName: "Symptoms");

            migrationBuilder.RenameTable(
                name: "ProductQuantity",
                newName: "ProductQuantities");

            migrationBuilder.RenameIndex(
                name: "IX_Symptom_SymptomsLogId",
                table: "Symptoms",
                newName: "IX_Symptoms_SymptomsLogId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductQuantity_ProductId",
                table: "ProductQuantities",
                newName: "IX_ProductQuantities_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductQuantity_MealLogId",
                table: "ProductQuantities",
                newName: "IX_ProductQuantities_MealLogId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Symptoms",
                table: "Symptoms",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductQuantities",
                table: "ProductQuantities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductQuantities_MealLogs_MealLogId",
                table: "ProductQuantities",
                column: "MealLogId",
                principalTable: "MealLogs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductQuantities_Products_ProductId",
                table: "ProductQuantities",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Symptoms_SymptomsLogs_SymptomsLogId",
                table: "Symptoms",
                column: "SymptomsLogId",
                principalTable: "SymptomsLogs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductQuantities_MealLogs_MealLogId",
                table: "ProductQuantities");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductQuantities_Products_ProductId",
                table: "ProductQuantities");

            migrationBuilder.DropForeignKey(
                name: "FK_Symptoms_SymptomsLogs_SymptomsLogId",
                table: "Symptoms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Symptoms",
                table: "Symptoms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductQuantities",
                table: "ProductQuantities");

            migrationBuilder.RenameTable(
                name: "Symptoms",
                newName: "Symptom");

            migrationBuilder.RenameTable(
                name: "ProductQuantities",
                newName: "ProductQuantity");

            migrationBuilder.RenameIndex(
                name: "IX_Symptoms_SymptomsLogId",
                table: "Symptom",
                newName: "IX_Symptom_SymptomsLogId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductQuantities_ProductId",
                table: "ProductQuantity",
                newName: "IX_ProductQuantity_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductQuantities_MealLogId",
                table: "ProductQuantity",
                newName: "IX_ProductQuantity_MealLogId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Symptom",
                table: "Symptom",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductQuantity",
                table: "ProductQuantity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductQuantity_MealLogs_MealLogId",
                table: "ProductQuantity",
                column: "MealLogId",
                principalTable: "MealLogs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductQuantity_Products_ProductId",
                table: "ProductQuantity",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Symptom_SymptomsLogs_SymptomsLogId",
                table: "Symptom",
                column: "SymptomsLogId",
                principalTable: "SymptomsLogs",
                principalColumn: "Id");
        }
    }
}
