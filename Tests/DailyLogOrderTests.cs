using AutoMapper;
using Core;
using Core.Services;
using DataAccess;
using DataAccess.Entities;
using DataAccess.Enums;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class DailyLogOrderTests
    {
        [Fact]
        public async Task GetDailyLogsByDate_orders_meals_and_symptoms_by_time()
        {
            var options = new DbContextOptionsBuilder<FodmapLogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            await using var context = new FodmapLogDbContext(options);
            const string userId = "user-a";
            var day = new DateTime(2026, 11, 17);

            context.MealLogs.Add(new MealLog
            {
                Date = day.AddHours(16),
                UserId = userId,
                ProductQuantity =
                [
                    new ProductQuantity
                    {
                        Quantity = 100,
                        Product = new Product { Name = "pork" },
                        Unit = new Unit { Name = "g" }
                    }
                ]
            });
            context.MealLogs.Add(new MealLog
            {
                Date = day.AddHours(10),
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
            context.SymptomsLogs.Add(new SymptomsLog
            {
                Date = day.AddHours(10).AddMinutes(30),
                UserId = userId,
                Symptoms =
                [
                    new Symptom
                    {
                        SymptomType = new SymptomType { Name = "Nausea" },
                        SymptomScale = SymptomScale.Mild
                    }
                ]
            });
            context.SymptomsLogs.Add(new SymptomsLog
            {
                Date = day.AddHours(12),
                UserId = userId,
                Symptoms =
                [
                    new Symptom
                    {
                        SymptomType = new SymptomType { Name = "Diarrhea" },
                        SymptomScale = SymptomScale.Low
                    }
                ]
            });
            await context.SaveChangesAsync();

            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<mapperConfig>()).CreateMapper();
            var service = new FodmapLogService(new FodmapLogRepository(context), mapper);

            var logs = (await service.GetDailyLogsByDate(day, userId, CancellationToken.None)).ToList();

            Assert.Equal(4, logs.Count);
            Assert.Equal(new TimeSpan(10, 0, 0), logs[0].Date.TimeOfDay);
            Assert.NotNull(logs[0].MealLog);
            Assert.Equal(new TimeSpan(10, 30, 0), logs[1].Date.TimeOfDay);
            Assert.NotNull(logs[1].SymptomsLog);
            Assert.Equal(new TimeSpan(12, 0, 0), logs[2].Date.TimeOfDay);
            Assert.NotNull(logs[2].SymptomsLog);
            Assert.Equal(new TimeSpan(16, 0, 0), logs[3].Date.TimeOfDay);
            Assert.NotNull(logs[3].MealLog);
        }
    }
}
