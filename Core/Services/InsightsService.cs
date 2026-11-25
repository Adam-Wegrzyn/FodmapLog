using Core.Insights;
using Core.Interfaces;
using Data.Common.DTO;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace Core.Services
{
    public class InsightsService : IInsightsService
    {
        private readonly IFodmapLogRepository _logs;
        private readonly IInsightFeedbackRepository _feedback;

        public InsightsService(IFodmapLogRepository logs, IInsightFeedbackRepository feedback)
        {
            _logs = logs;
            _feedback = feedback;
        }

        public async Task<InsightsResponseDto> GetInsights(string userId, int days, CancellationToken cancellationToken)
        {
            if (days < InsightsThresholds.MinWindowDays || days > InsightsThresholds.MaxWindowDays)
            {
                throw new ArgumentOutOfRangeException(nameof(days));
            }

            var today = DateTime.UtcNow.Date;
            var (start, end, symptomEnd) = InsightsCalculator.WindowFor(today, days);

            var meals = await _logs.GetMealLogsByDateRange(start, end, userId, cancellationToken);
            var symptoms = await _logs.GetSymptomsLogsByDateRange(start, symptomEnd, userId, cancellationToken);

            var mealInput = meals.Select(m => new InsightMeal(
                m.Date,
                (m.ProductQuantity ?? new List<ProductQuantity>())
                    .Select(pq => pq.Product?.Name ?? string.Empty)
                    .ToList())).ToList();

            var symptomInput = symptoms.Select(log => new InsightSymptomLog(
                log.Date,
                InsightsCalculator.InProgressWindow(log.Date, start, end),
                (log.Symptoms ?? new List<Symptom>()).Select(s => new InsightSymptom(
                    s.SymptomType?.Name,
                    s.SymptomType == null ? null : s.SymptomType.ScaleKind,
                    (int)s.SymptomScale)).ToList())).ToList();

            return InsightsCalculator.Compute(mealInput, symptomInput, days);
        }

        public Task SaveFeedback(string userId, InsightFeedbackRequestDto request, CancellationToken cancellationToken)
        {
            var foodKey = Clip(request.FoodKey, 200);
            if (foodKey.Length == 0)
            {
                throw new ArgumentException("foodKey is required.");
            }

            var row = new InsightFeedback
            {
                UserId = userId,
                FoodKey = foodKey,
                ProductName = Clip(request.Product, 200),
                Helpful = request.Helpful,
                CreatedAtUtc = DateTime.UtcNow
            };

            return _feedback.Add(row, cancellationToken);
        }

        private static string Clip(string? value, int max)
        {
            var text = (value ?? string.Empty).Trim();
            return text.Length <= max ? text : text[..max];
        }
    }
}
