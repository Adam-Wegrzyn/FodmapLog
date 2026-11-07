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
            // Hand-written data ops must be SQL: UpdateData/InsertData need a designer model
            // and fail with "no entity type mapped to the table 'SymptomTypes'".
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.SymptomTypes', 'ScaleKind') IS NULL
                BEGIN
                    ALTER TABLE [SymptomTypes] ADD [ScaleKind] int NOT NULL
                        CONSTRAINT [DF_SymptomTypes_ScaleKind] DEFAULT 0;
                END

                UPDATE [SymptomTypes]
                SET [ScaleKind] = 1
                WHERE [UserId] IS NULL AND [Id] IN (11, 14, 15, 16, 18, 19, 20, 21);

                SET IDENTITY_INSERT [SymptomTypes] ON;
                INSERT INTO [SymptomTypes] ([Id], [Name], [ScaleKind])
                SELECT v.[Id], v.[Name], v.[ScaleKind]
                FROM (VALUES
                    (22, N'Brain Fog', 0),
                    (23, N'Dizziness', 0),
                    (24, N'Anxiety', 0),
                    (25, N'Joint Pain', 0),
                    (26, N'Itching', 0),
                    (27, N'Urgency', 0),
                    (28, N'Incomplete Evacuation', 0),
                    (29, N'Mucus', 0),
                    (30, N'Palpitations', 0),
                    (31, N'Irritability', 0)
                ) AS v([Id], [Name], [ScaleKind])
                WHERE NOT EXISTS (SELECT 1 FROM [SymptomTypes] t WHERE t.[Id] = v.[Id])
                  AND NOT EXISTS (SELECT 1 FROM [SymptomTypes] t WHERE t.[UserId] IS NULL AND t.[Name] = v.[Name]);
                SET IDENTITY_INSERT [SymptomTypes] OFF;

                INSERT INTO [SymptomTypes] ([Name], [ScaleKind])
                SELECT v.[Name], v.[ScaleKind]
                FROM (VALUES
                    (N'Brain Fog', 0),
                    (N'Dizziness', 0),
                    (N'Anxiety', 0),
                    (N'Joint Pain', 0),
                    (N'Itching', 0),
                    (N'Urgency', 0),
                    (N'Incomplete Evacuation', 0),
                    (N'Mucus', 0),
                    (N'Palpitations', 0),
                    (N'Irritability', 0)
                ) AS v([Name], [ScaleKind])
                WHERE NOT EXISTS (SELECT 1 FROM [SymptomTypes] t WHERE t.[UserId] IS NULL AND t.[Name] = v.[Name]);

                UPDATE s
                SET s.SymptomTypeId = seed.Id
                FROM [Symptoms] s
                INNER JOIN [SymptomTypes] personal ON s.SymptomTypeId = personal.Id AND personal.UserId IS NOT NULL
                INNER JOIN [SymptomTypes] seed ON seed.UserId IS NULL
                    AND LOWER(seed.Name) = LOWER(personal.Name)
                    AND seed.Name IN (
                        N'Brain Fog', N'Dizziness', N'Anxiety', N'Joint Pain', N'Itching',
                        N'Urgency', N'Incomplete Evacuation', N'Mucus', N'Palpitations', N'Irritability'
                    );

                DELETE FROM [SymptomTypes]
                WHERE UserId IS NOT NULL
                  AND EXISTS (
                    SELECT 1 FROM [SymptomTypes] seed
                    WHERE seed.UserId IS NULL
                      AND LOWER(seed.Name) = LOWER([SymptomTypes].Name)
                      AND seed.Name IN (
                        N'Brain Fog', N'Dizziness', N'Anxiety', N'Joint Pain', N'Itching',
                        N'Urgency', N'Incomplete Evacuation', N'Mucus', N'Palpitations', N'Irritability'
                    )
                  );

                DECLARE @maxId int = (SELECT MAX(Id) FROM [SymptomTypes]);
                IF @maxId IS NOT NULL
                    DBCC CHECKIDENT ('SymptomTypes', RESEED, @maxId);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [SymptomTypes]
                WHERE [UserId] IS NULL AND [Name] IN (
                    N'Brain Fog', N'Dizziness', N'Anxiety', N'Joint Pain', N'Itching',
                    N'Urgency', N'Incomplete Evacuation', N'Mucus', N'Palpitations', N'Irritability'
                );

                IF COL_LENGTH('dbo.SymptomTypes', 'ScaleKind') IS NOT NULL
                BEGIN
                    ALTER TABLE [SymptomTypes] DROP CONSTRAINT [DF_SymptomTypes_ScaleKind];
                    ALTER TABLE [SymptomTypes] DROP COLUMN [ScaleKind];
                END
                """);
        }
    }
}
