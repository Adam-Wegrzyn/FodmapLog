using Core.Interfaces;
using Data.Common.DTO;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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

        [HttpGet]
        [Route("symptomTypes")]
        public async Task<IActionResult> symptomTypes(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new Core.CQRS.GetSymptomTypesQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
