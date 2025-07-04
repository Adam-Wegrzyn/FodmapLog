using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SymptomType",
                table: "Symptoms",
                newName: "SymptomTypeId");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "ProductQuantities",
                newName: "UnitId");

            migrationBuilder.CreateTable(
                name: "SymptomTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SymptomTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Symptoms_SymptomTypeId",
                table: "Symptoms",
                column: "SymptomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuantities_UnitId",
                table: "ProductQuantities",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductQuantities_Units_UnitId",
                table: "ProductQuantities",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Symptoms_SymptomTypes_SymptomTypeId",
                table: "Symptoms",
                column: "SymptomTypeId",
                principalTable: "SymptomTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductQuantities_Units_UnitId",
                table: "ProductQuantities");

            migrationBuilder.DropForeignKey(
                name: "FK_Symptoms_SymptomTypes_SymptomTypeId",
                table: "Symptoms");

            migrationBuilder.DropTable(
                name: "SymptomTypes");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Symptoms_SymptomTypeId",
                table: "Symptoms");

            migrationBuilder.DropIndex(
                name: "IX_ProductQuantities_UnitId",
                table: "ProductQuantities");

            migrationBuilder.RenameColumn(
                name: "SymptomTypeId",
                table: "Symptoms",
                newName: "SymptomType");

            migrationBuilder.RenameColumn(
                name: "UnitId",
                table: "ProductQuantities",
                newName: "Unit");
        }
    }
}
