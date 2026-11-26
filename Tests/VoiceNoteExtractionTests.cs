using AutoMapper;
using Core;
using Core.Ai;
using Core.Services;
using Data.Common.DTO;
using DataAccess;
using DataAccess.Entities;
using DataAccess.Enums;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class VoiceNoteExtractionTests
    {
        private static readonly RecordingClock Bangkok = new(
            new DateTime(2026, 11, 26, 10, 47, 0),
            UserClock.FromClient("Asia/Bangkok", 420));

        [Fact]
        public async Task Normalizer_rewrites_utc_now_using_the_spoken_phrases()
        {
            await using var context = NewContext();
            context.SymptomTypes.Add(new SymptomType { Id = 5, Name = "Bloating" });
            context.SymptomTypes.Add(new SymptomType { Id = 6, Name = "Abdominal Pain" });
            context.Units.Add(new Unit { Id = 1, Name = "Gram" });
            context.Units.Add(new Unit { Id = 9, Name = "Piece" });
            await context.SaveChangesAsync();

            var normalizer = new AiDailyLogNormalizer(new FodmapLogRepository(context));
            var outcome = await normalizer.NormalizeAsync(BangkokExampleJson(), null, "user-a", CancellationToken.None, Bangkok);

            Assert.True(outcome.Success);
            Assert.Equal(4, outcome.Logs.Count);

            Assert.Equal(new DateTime(2026, 11, 25, 20, 0, 0), outcome.Logs[0].Date);
            Assert.Equal("ryż", outcome.Logs[0].MealLog!.ProductQuantity!.First().Product!.Name);
            Assert.Equal(4, outcome.Logs[0].MealLog.ProductQuantity!.Count());

            Assert.Equal(new DateTime(2026, 11, 25, 21, 30, 0), outcome.Logs[1].Date);
            var heaviness = Assert.Single(outcome.Logs[1].SymptomsLog!.Symptoms);
            Assert.Equal(5, heaviness.SymptomType.Id);
            Assert.Equal("Bloating", heaviness.SymptomType.Name);
            Assert.Equal("lekka ciężkość", heaviness.SpokenName);
            Assert.Equal(SymptomScale.Low, heaviness.SymptomScale);

            Assert.Equal(new DateTime(2026, 11, 25, 23, 0, 0), outcome.Logs[2].Date);
            Assert.Equal("elektrolity", outcome.Logs[2].MealLog!.ProductQuantity!.Single().Product!.Name);

            Assert.Equal(new DateTime(2026, 11, 26, 7, 30, 0), outcome.Logs[3].Date);
            var discomfort = Assert.Single(outcome.Logs[3].SymptomsLog!.Symptoms);
            Assert.Equal(5, discomfort.SymptomType.Id);
            Assert.Equal("lekki dyskomfort żołądka", discomfort.SpokenName);
            Assert.Equal(SymptomScale.Low, discomfort.SymptomScale);
        }

        [Fact]
        public async Task Stored_utc_instant_comes_back_as_the_users_local_time_on_the_right_day()
        {
            await using var context = NewContext();
            context.SymptomTypes.Add(new SymptomType { Id = 5, Name = "Bloating" });
            await context.SaveChangesAsync();

            var clock = UserClock.FromClient("Asia/Bangkok", 420);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<mapperConfig>()).CreateMapper();
            var service = new FodmapLogService(new FodmapLogRepository(context), mapper, clock);
            const string userId = "user-a";

            await service.AddMealLog(new MealLogDto
            {
                Date = new DateTime(2026, 11, 25, 20, 0, 0),
                ProductQuantity =
                [
                    new ProductQuantityDto
                    {
                        Quantity = 100,
                        Product = new ProductDto { Name = "ryż" },
                        Unit = new Unit { Name = "Gram" }
                    }
                ]
            }, userId, CancellationToken.None);

            await service.AddSymptomsLog(new SymptomsLogDto
            {
                Date = new DateTime(2026, 11, 26, 1, 0, 0),
                Symptoms =
                [
                    new SymptomDto
                    {
                        SymptomScale = SymptomScale.Low,
                        SpokenName = "lekki dyskomfort żołądka",
                        SymptomType = new SymptomTypeDto { Id = 5, Name = "Bloating" }
                    }
                ]
            }, userId, CancellationToken.None);

            context.MealLogs.Add(new MealLog
            {
                Date = new DateTime(2026, 11, 25, 10, 0, 0),
                UtcInstant = false,
                UserId = userId,
                ProductQuantity =
                [
                    new ProductQuantity
                    {
                        Quantity = 1,
                        Product = new Product { Name = "coffee" },
                        Unit = new Unit { Name = "cup" }
                    }
                ]
            });
            await context.SaveChangesAsync();

            var storedMeal = context.MealLogs.Single(m => m.UtcInstant);
            Assert.Equal(new DateTime(2026, 11, 25, 13, 0, 0), storedMeal.Date);
            var storedSymptom = context.SymptomsLogs.Single();
            Assert.True(storedSymptom.UtcInstant);
            Assert.Equal(new DateTime(2026, 11, 25, 18, 0, 0), storedSymptom.Date);

            var nov25 = (await service.GetDailyLogsByDate(new DateTime(2026, 11, 25), userId, CancellationToken.None)).ToList();
            Assert.Equal(2, nov25.Count);
            Assert.Equal(new TimeSpan(10, 0, 0), nov25[0].Date.TimeOfDay);
            Assert.Equal(new TimeSpan(20, 0, 0), nov25[1].Date.TimeOfDay);
            Assert.Equal(new DateTime(2026, 11, 25), nov25[1].Date.Date);

            var nov26 = (await service.GetDailyLogsByDate(new DateTime(2026, 11, 26), userId, CancellationToken.None)).ToList();
            var morning = Assert.Single(nov26);
            Assert.Equal(new DateTime(2026, 11, 26, 1, 0, 0), morning.Date);
            Assert.Equal("lekki dyskomfort żołądka", morning.SymptomsLog!.Symptoms.Single().SpokenName);
        }

        private static FodmapLogDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<FodmapLogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FodmapLogDbContext(options);
        }

        private static string BangkokExampleJson() =>
            """
            {
              "events": [
                {
                  "timeExpression": "wczoraj o dwudziestej",
                  "date": "2026-11-26T03:47:00",
                  "mealLog": {
                    "date": "2026-11-26T03:47:00",
                    "productQuantity": [
                      { "product": { "name": "ryż" }, "quantity": 100, "unit": { "name": "Gram" } },
                      { "product": { "name": "deser kokosowy" }, "quantity": 1, "unit": { "name": "Piece" } },
                      { "product": { "name": "kurczak po tajsku" }, "quantity": 100, "unit": { "name": "Gram" } },
                      { "product": { "name": "wieprzowina" }, "quantity": 1, "unit": { "name": "Piece" } }
                    ]
                  },
                  "symptomsLog": null
                },
                {
                  "timeExpression": "półtorej godziny później",
                  "date": "2026-11-26T03:47:00",
                  "mealLog": null,
                  "symptomsLog": {
                    "date": "2026-11-26T03:47:00",
                    "symptoms": [
                      { "symptomType": { "name": "Abdominal Pain" }, "spokenName": "lekka ciężkość", "symptomScale": 4 }
                    ]
                  }
                },
                {
                  "timeExpression": "przed snem",
                  "date": "2026-11-26T03:47:00",
                  "mealLog": {
                    "date": "2026-11-26T03:47:00",
                    "productQuantity": [
                      { "product": { "name": "elektrolity" }, "quantity": 1, "unit": { "name": "Piece" } }
                    ]
                  },
                  "symptomsLog": null
                },
                {
                  "timeExpression": "rano następnego dnia",
                  "date": "2026-11-26T03:47:00",
                  "mealLog": null,
                  "symptomsLog": {
                    "date": "2026-11-26T03:47:00",
                    "symptoms": [
                      { "symptomType": { "name": "Ból brzucha" }, "spokenName": "lekki dyskomfort żołądka", "symptomScale": 3 }
                    ]
                  }
                }
              ]
            }
            """;
    }
}
