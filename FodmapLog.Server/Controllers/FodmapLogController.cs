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
        public IActionResult GetMealLogById(int id, CancellationToken cancellationToken)
        {
            var result = _fodmapLogService.GetMealLogById(id, cancellationToken);
            return Ok(result);
        }


        [HttpGet]
        [Route("getAllMealLogs")]
        public IActionResult GetAllMealLogs(CancellationToken cancellationToken)
        {
            var result = _fodmapLogService.GetAllMealLogs(cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("addMealLog")]
        public IActionResult AddMealLog([FromBody] MealLogDto mealLogDto, CancellationToken cancellationToken)
        {

            var result = _fodmapLogService.AddMealLog(mealLogDto, cancellationToken);
            return Ok(result);
        }        
        
    }
}
