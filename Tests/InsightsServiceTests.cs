using Core.Services;
using Data.Common.DTO;
using DataAccess;
using DataAccess.Entities;
using DataAccess.Enums;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class InsightsServiceTests
    {
        [Fact]
        public async Task GetInsights_uses_only_the_signed_in_users_logs()
        {
            await using var context = NewContext();
            var today = DateTime.UtcNow.Date;
            AddMeal(context, "user-a", today.AddHours(9), "rice");
            AddMeal(context, "user-a", today.AddDays(-1).AddHours(9), "rice");

            for (var d = 0; d < 8; d++)
            {
                var at = today.AddDays(-d).AddHours(8);
                AddMeal(context, "user-b", at, "ghostpepper");
                context.SymptomsLogs.Add(new SymptomsLog
                {
                    UserId = "user-b",
                    Date = at.AddHours(4),
                    Symptoms = new List<Symptom>
                    {
                        new()
                        {
                            SymptomType = new SymptomType { Name = "Bloating", ScaleKind = SymptomScaleKind.Negative },
                            SymptomScale = SymptomScale.Severe
                        }
                    }
                });
            }

            await context.SaveChangesAsync();
            var service = new InsightsService(new FodmapLogRepository(context), new InsightFeedbackRepository(context));

            var result = await service.GetInsights("user-a", 14, CancellationToken.None);

            Assert.Equal("locked", result.Status);
            Assert.Equal(2, result.Progress.Meals);
            Assert.Empty(result.Findings);
            Assert.DoesNotContain(result.RarelyFollowed, r => r.FoodKey == "ghostpepper");
        }

        [Fact]
        public async Task SaveFeedback_persists_the_users_vote_and_rejects_a_blank_key()
        {
            await using var context = NewContext();
            var service = new InsightsService(new FodmapLogRepository(context), new InsightFeedbackRepository(context));

            await service.SaveFeedback("user-a", new InsightFeedbackRequestDto
            {
                FoodKey = " onion ",
                Product = "Cebula",
                Helpful = true
            }, CancellationToken.None);

            var row = Assert.Single(context.InsightFeedback);
            Assert.Equal("user-a", row.UserId);
            Assert.Equal("onion", row.FoodKey);
            Assert.Equal("Cebula", row.ProductName);
            Assert.True(row.Helpful);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.SaveFeedback("user-a", new InsightFeedbackRequestDto { FoodKey = "  " }, CancellationToken.None));
        }

        private static FodmapLogDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<FodmapLogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FodmapLogDbContext(options);
        }

        private static void AddMeal(FodmapLogDbContext context, string userId, DateTime at, string product)
        {
            context.MealLogs.Add(new MealLog
            {
                UserId = userId,
                Date = at,
                ProductQuantity = new List<ProductQuantity>
                {
                    new()
                    {
                        Quantity = 1,
                        Product = new Product { Name = product },
                        Unit = new Unit { Name = "Piece" }
                    }
                }
            });
        }
    }
}
