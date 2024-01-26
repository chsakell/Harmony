using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Application.Features.Automations.Commands.RemoveAutomation;
using Harmony.Application.Features.Automations.Commands.ToggleAutomation;
using Harmony.Application.Features.Automations.Queries.GetAutomations;
using Harmony.Application.Features.Automations.Queries.GetAutomationTemplates;
using Harmony.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Automations.Controllers
{
    public class AutomationsController : BaseApiController<AutomationsController>
    {
        [HttpGet("{boardId:guid}/types/{type:int}")]
        public async Task<IActionResult> GetAutomations(Guid boardId, AutomationType type)
        {
            return Ok(await _mediator.Send(new GetAutomationsQuery(type, boardId)));
        }
    }
}
