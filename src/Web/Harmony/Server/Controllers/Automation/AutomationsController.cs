using Harmony.Application.Features.Automations.Queries.GetAutomations;
using Harmony.Application.Features.Automations.Queries.GetAutomationTemplates;
using Harmony.Domain.Enums;
using Harmony.Server.Controllers.Management;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Automation
{
    public class AutomationsController : BaseApiController<CardsController>
    {

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            return Ok(await _mediator.Send(new GetAutomationTemplatesQuery()));
        }

        [HttpGet("{boardId:guid}/types/{type:int}")]
        public async Task<IActionResult> GetAutomations(Guid boardId, AutomationType type)
        {
            return Ok(await _mediator.Send(new GetAutomationsQuery(type, boardId)));
        }
    }
}
