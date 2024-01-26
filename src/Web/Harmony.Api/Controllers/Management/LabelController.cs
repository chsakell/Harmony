using Harmony.Application.Features.Labels.Commands.CreateCardLabel;
using Harmony.Application.Features.Labels.Commands.RemoveCardLabel;
using Harmony.Application.Features.Labels.Commands.UpdateTitle;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers.Management
{
    /// <summary>
    /// Controller for Label operations
    /// </summary>
    public class LabelsController : BaseApiController<LabelsController>
    {
        [HttpPost]
        public async Task<IActionResult> Post(CreateCardLabelCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("{id:guid}/title")]
        public async Task<IActionResult> Update(Guid id, UpdateLabelTitleCommand request)
        {
            return Ok(await _mediator.Send(request));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await _mediator.Send(new RemoveCardLabelCommand(id)));
        }
    }
}
