using DataAccess.Entities;
using DataAccess.Interfaces;

namespace DataAccess.Repositories
{
    public class InsightFeedbackRepository : IInsightFeedbackRepository
    {
        private readonly FodmapLogDbContext _context;

        public InsightFeedbackRepository(FodmapLogDbContext context)
        {
            _context = context;
        }

        public async Task Add(InsightFeedback feedback, CancellationToken cancellationToken)
        {
            _context.InsightFeedback.Add(feedback);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
