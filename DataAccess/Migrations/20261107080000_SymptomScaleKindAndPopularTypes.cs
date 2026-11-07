using DataAccess;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    [DbContext(typeof(FodmapLogDbContext))]
    [Migration("20261107080000_SymptomScaleKindAndPopularTypes")]
    public partial class SymptomScaleKindAndPopularTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScaleKind",
                table: "SymptomTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            foreach (var id in new[] { 11, 14, 15, 16, 18, 19, 20, 21 })
            {
                migrationBuilder.UpdateData(
                    table: "SymptomTypes",
                    keyColumn: "Id",
                    keyValue: id,
                    column: "ScaleKind",
                    value: 1);
            }

            migrationBuilder.InsertData(
                table: "SymptomTypes",
                columns: new[] { "Id", "Name", "ScaleKind" },
                values: new object[,]
                {
                    { 22, "Brain Fog", 0 },
                    { 23, "Dizziness", 0 },
                    { 24, "Anxiety", 0 },
                    { 25, "Joint Pain", 0 },
                    { 26, "Itching", 0 },
                    { 27, "Urgency", 0 },
                    { 28, "Incomplete Evacuation", 0 },
                    { 29, "Mucus", 0 },
                    { 30, "Palpitations", 0 },
                    { 31, "Irritability", 0 }
                });

            // Fold custom names that now exist in the catalog onto the seed rows.
            migrationBuilder.Sql("""
                UPDATE s
                SET s.SymptomTypeId = seed.Id
                FROM Symptoms s
                INNER JOIN SymptomTypes personal ON s.SymptomTypeId = personal.Id AND personal.UserId IS NOT NULL
                INNER JOIN SymptomTypes seed ON seed.UserId IS NULL
                    AND LOWER(seed.Name) = LOWER(personal.Name)
                    AND seed.Id BETWEEN 22 AND 31;

                DELETE FROM SymptomTypes
                WHERE UserId IS NOT NULL
                  AND EXISTS (
                    SELECT 1 FROM SymptomTypes seed
                    WHERE seed.UserId IS NULL
                      AND LOWER(seed.Name) = LOWER(SymptomTypes.Name)
                      AND seed.Id BETWEEN 22 AND 31
                  );

                DECLARE @maxId int = (SELECT MAX(Id) FROM SymptomTypes);
                IF @maxId IS NOT NULL
                    DBCC CHECKIDENT ('SymptomTypes', RESEED, @maxId);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "SymptomTypes", keyColumn: "Id", keyValue: 22);
            migrationBuilder.DeleteData(table: "SymptomTypes", keyColumn: "Id", keyValue: 23);
            migrationBuilder.DeleteData(table: "SymptomTypes", keyColumn: "Id", keyValue: 24);
            migrationBuilder.DeleteData(table: "SymptomTypes", keyColumn: "Id", keyValue: 25);
            migrationBuilder.DeleteData(table: "SymptomTypes", keyColumn: "Id", keyValue: 26);
            migrationBuilder.DeleteData(table: "SymptomTypes", keyColumn: "Id", keyValue: 27);
            migrationBuilder.DeleteData(table: "SymptomTypes", keyColumn: "Id", keyValue: 28);
            migrationBuilder.DeleteData(table: "SymptomTypes", keyColumn: "Id", keyValue: 29);
            migrationBuilder.DeleteData(table: "SymptomTypes", keyColumn: "Id", keyValue: 30);
            migrationBuilder.DeleteData(table: "SymptomTypes", keyColumn: "Id", keyValue: 31);

            migrationBuilder.DropColumn(
                name: "ScaleKind",
                table: "SymptomTypes");
        }
    }
}
