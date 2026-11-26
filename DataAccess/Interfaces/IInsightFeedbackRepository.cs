using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface IInsightFeedbackRepository
    {
        Task Add(InsightFeedback feedback, CancellationToken cancellationToken);
    }
}
