using Core.Interfaces;
using Data.Common.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FodmapLogController : ControllerBase
    {
        private IFodmapLogService _fodmapLogService;
        public FodmapLogController(IFodmapLogService fodmapLogService)
        {

            _fodmapLogService = fodmapLogService;

        }
        [HttpGet]
        [Route("getMealLogById/{id}")]
        public async Task<IActionResult> GetMealLogById(int id, CancellationToken cancellationToken)
        {
            var result = await  _fodmapLogService.GetMealLogById(id, cancellationToken);
            return Ok(result);
        }


        [HttpGet]
        [Route("getAllMealLogs")]
        public async Task<IActionResult> GetAllMealLogs(CancellationToken cancellationToken)
        {
            var result = await _fodmapLogService.GetAllMealLogs(cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Route("getDailyLogsByDate/{date}")]
        public async Task<IActionResult> GetDailyLogsByDate(string date, CancellationToken cancellationToken)
        {
            var result = await _fodmapLogService.GetDailyLogsByDate(DateTime.Parse(date), cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("addMealLog")]
        public async Task<IActionResult> AddMealLog([FromBody] MealLogDto mealLogDto, CancellationToken cancellationToken)
        {

            var result = await _fodmapLogService.AddMealLog(mealLogDto, cancellationToken);
            return Ok(result);
        }

        [HttpPut]
        [Route("updateMealLog")]
        public async Task<IActionResult> UpdateMealLog([FromBody] MealLogDto mealLogDto, CancellationToken cancellationToken)
        {

            var result = await _fodmapLogService.UpdateMealLog(mealLogDto, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("addSymptomsLog")]
        public async Task<IActionResult> AddSymptomsLog([FromBody] SymptomsLogDto symptomsLogDto, CancellationToken cancellationToken)
        {
            var result = await _fodmapLogService.AddSymptomsLog(symptomsLogDto, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Route("getSymptomsLogById/{id}")]
        public async Task<IActionResult> GetSymptomsLogById(int id, CancellationToken cancellationToken)
        {
            var result = await _fodmapLogService.GetSymptomsLogById(id, cancellationToken);
            return Ok(result);
        }

        [HttpPut]
        [Route("updateSymptomsLog")]
        public async Task<IActionResult> UpdateSymptomsLog([FromBody] SymptomsLogDto symptomsLogDto, CancellationToken cancellationToken)
        {
            var result = await _fodmapLogService.UpdateSymptomsLog(symptomsLogDto, cancellationToken);
            return Ok(result);
        }

    }
}
