using DataAccess.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var result = await _mediator.Send(new Core.CQRS.GetSymptomTypesQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddSymptomType([FromBody] SymptomType symptomType, CancellationToken cancellationToken)
        {
            if (symptomType == null || string.IsNullOrWhiteSpace(symptomType.Name))
            {
                return BadRequest("Invalid symptom type data.");
            }
            return Created();
            //    await _mediator.Send(new Core.CQRS.AddSymptomTypeCommand(symptomType), cancellationToken);
            //    return CreatedAtAction(nameof(GetAll), new { id = symptomType.Id }, symptomType);
            //}
        }
    }
}