using Core.Interfaces;
using Data.Common.DTO;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("diary")]
    public class FodmapLogController : ControllerBase
    {
        private readonly IFodmapLogService _fodmapLogService;
        private readonly IMediator _mediator;

        public FodmapLogController(IFodmapLogService fodmapLogService, IMediator mediator)
        {
            _fodmapLogService = fodmapLogService;
            _mediator = mediator;
        }

        [HttpGet]
        [Route("getMealLogById/{id}")]
        public async Task<IActionResult> GetMealLogById(int id, CancellationToken cancellationToken)
        {
            var userId = this.RequireUserId();
            var result = await _fodmapLogService.GetMealLogById(id, userId, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("getAllMealLogs")]
        public async Task<IActionResult> GetAllMealLogs(CancellationToken cancellationToken)
        {
            var userId = this.RequireUserId();
            var result = await _fodmapLogService.GetAllMealLogs(userId, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Route("getDailyLogsByDate/{date}")]
        public async Task<IActionResult> GetDailyLogsByDate(string date, CancellationToken cancellationToken)
        {
            var userId = this.RequireUserId();
            var result = await _fodmapLogService.GetDailyLogsByDate(DateTime.Parse(date), userId, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Route("getDailyLogsByDateRange/{from}/{to}")]
        public async Task<IActionResult> GetDailyLogsByDateRange(string from, string to, CancellationToken cancellationToken)
        {
            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            {
                return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });
            }

            if (toDate.Date < fromDate.Date)
            {
                return BadRequest(new { error = "End date must be on or after start date." });
            }

            // Keep exports bounded for mobile/API cost.
            if ((toDate.Date - fromDate.Date).TotalDays > 366)
            {
                return BadRequest(new { error = "Date range cannot exceed 366 days." });
            }

            var userId = this.RequireUserId();
            var result = await _fodmapLogService.GetDailyLogsByDateRange(fromDate, toDate, userId, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("addMealLog")]
        public async Task<IActionResult> AddMealLog([FromBody] MealLogDto mealLogDto, CancellationToken cancellationToken)
        {
            var userId = this.RequireUserId();
            var result = await _fodmapLogService.AddMealLog(mealLogDto, userId, cancellationToken);
            return Ok(result);
        }

        [HttpPut]
        [Route("updateMealLog")]
        public async Task<IActionResult> UpdateMealLog([FromBody] MealLogDto mealLogDto, CancellationToken cancellationToken)
        {
            var userId = this.RequireUserId();
            try
            {
                var result = await _fodmapLogService.UpdateMealLog(mealLogDto, userId, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [Route("deleteMealLog/{id}")]
        public async Task<IActionResult> DeleteMealLog(int id, CancellationToken cancellationToken)
        {
            var userId = this.RequireUserId();
            var result = await _fodmapLogService.DeleteMealLog(id, userId, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("addSymptomsLog")]
        public async Task<IActionResult> AddSymptomsLog([FromBody] SymptomsLogDto symptomsLogDto, CancellationToken cancellationToken)
        {
            var userId = this.RequireUserId();
            var result = await _fodmapLogService.AddSymptomsLog(symptomsLogDto, userId, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Route("getSymptomsLogById/{id}")]
        public async Task<IActionResult> GetSymptomsLogById(int id, CancellationToken cancellationToken)
        {
            var userId = this.RequireUserId();
            var result = await _fodmapLogService.GetSymptomsLogById(id, userId, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPut]
        [Route("updateSymptomsLog")]
        public async Task<IActionResult> UpdateSymptomsLog([FromBody] SymptomsLogDto symptomsLogDto, CancellationToken cancellationToken)
        {
            var userId = this.RequireUserId();
            try
            {
                var result = await _fodmapLogService.UpdateSymptomsLog(symptomsLogDto, userId, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [Route("deleteSymptomsLog/{id}")]
        public async Task<IActionResult> DeleteSymptomsLog(int id, CancellationToken cancellationToken)
        {
            var userId = this.RequireUserId();
            var result = await _fodmapLogService.DeleteSymptomsLog(id, userId, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("symptomTypes")]
        public async Task<IActionResult> symptomTypes(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new Core.CQRS.GetSymptomTypesQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
