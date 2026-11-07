using DataAccess.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SymptomTypesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SymptomTypesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var userId = this.RequireUserId();
            var result = await _mediator.Send(
                new Core.CQRS.GetSymptomTypesQuery { UserId = userId },
                cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddSymptomType(
            [FromBody] SymptomType symptomType,
            CancellationToken cancellationToken)
        {
            if (symptomType == null || string.IsNullOrWhiteSpace(symptomType.Name))
            {
                return BadRequest(new { error = "Symptom name is required." });
            }

            var userId = this.RequireUserId();
            var created = await _mediator.Send(
                new Core.CQRS.AddPersonalSymptomTypeCommand
                {
                    Name = symptomType.Name,
                    UserId = userId
                },
                cancellationToken);
            return Ok(created);
        }
    }
}
