using Data.Common.DTO;

namespace Core.Interfaces
{
    public interface IInsightsService
    {
        Task<InsightsResponseDto> GetInsights(string userId, int days, CancellationToken cancellationToken);
        Task SaveFeedback(string userId, InsightFeedbackRequestDto request, CancellationToken cancellationToken);
    }
}
