using Harmony.Application.Features.Automations.Queries.GetAutomationTemplates;
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
    }
}
