using Core.Insights;
using Core.Interfaces;
using Data.Common.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("diary")]
    public class InsightsController : ControllerBase
    {
        private readonly IInsightsService _insights;

        public InsightsController(IInsightsService insights)
        {
            _insights = insights;
        }

        [HttpGet]
        [Route("getInsights")]
        public async Task<IActionResult> GetInsights([FromQuery] int days = InsightsThresholds.DefaultWindowDays, CancellationToken cancellationToken = default)
        {
            if (days < InsightsThresholds.MinWindowDays || days > InsightsThresholds.MaxWindowDays)
            {
                return BadRequest(new
                {
                    error = $"days must be between {InsightsThresholds.MinWindowDays} and {InsightsThresholds.MaxWindowDays}."
                });
            }

            var userId = this.RequireUserId();
            var result = await _insights.GetInsights(userId, days, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("feedback")]
        public async Task<IActionResult> Feedback([FromBody] InsightFeedbackRequestDto? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Feedback body is required." });
            }

            var userId = this.RequireUserId();
            try
            {
                await _insights.SaveFeedback(userId, request, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            return Ok(new { saved = true });
        }
    }
}
