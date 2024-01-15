using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Application.Features.Automations.Commands.ToggleAutomation;
using Harmony.Application.Features.Automations.Queries.GetAutomations;
using Harmony.Application.Features.Automations.Queries.GetAutomationTemplates;
using Harmony.Application.Features.Labels.Commands.UpdateTitle;
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

        [HttpPost]
        public async Task<IActionResult> Create(CreateAutomationCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> Update(string id, ToggleAutomationCommand request)
        {
            return Ok(await _mediator.Send(request));
        }

        [HttpGet("{boardId:guid}/types/{type:int}")]
        public async Task<IActionResult> GetAutomations(Guid boardId, AutomationType type)
        {
            return Ok(await _mediator.Send(new GetAutomationsQuery(type, boardId)));
        }
    }
}
