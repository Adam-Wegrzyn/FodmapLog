using System;
using DataAccess;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    [DbContext(typeof(FodmapLogDbContext))]
    [Migration("20260902183000_AddUserIdToMealAndSymptomsLogs")]
    public partial class AddUserIdToMealAndSymptomsLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nullable so existing rows are isolated (UserId IS NULL) until manually backfilled.
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MealLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "SymptomsLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_UserId",
                table: "MealLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SymptomsLogs_UserId",
                table: "SymptomsLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_UserId_Date",
                table: "MealLogs",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_SymptomsLogs_UserId_Date",
                table: "SymptomsLogs",
                columns: new[] { "UserId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SymptomsLogs_UserId_Date",
                table: "SymptomsLogs");

            migrationBuilder.DropIndex(
                name: "IX_MealLogs_UserId_Date",
                table: "MealLogs");

            migrationBuilder.DropIndex(
                name: "IX_SymptomsLogs_UserId",
                table: "SymptomsLogs");

            migrationBuilder.DropIndex(
                name: "IX_MealLogs_UserId",
                table: "MealLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MealLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SymptomsLogs");
        }
    }
}
