using DataAccess;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    [DbContext(typeof(FodmapLogDbContext))]
    [Migration("20261105040000_AddUserIdToSymptomTypes")]
    public partial class AddUserIdToSymptomTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SymptomTypes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "SymptomTypes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SymptomTypes_UserId",
                table: "SymptomTypes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SymptomTypes_UserId_Name",
                table: "SymptomTypes",
                columns: new[] { "UserId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SymptomTypes_UserId_Name",
                table: "SymptomTypes");

            migrationBuilder.DropIndex(
                name: "IX_SymptomTypes_UserId",
                table: "SymptomTypes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SymptomTypes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SymptomTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
