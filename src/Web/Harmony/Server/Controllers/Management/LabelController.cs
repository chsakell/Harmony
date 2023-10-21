using Harmony.Application.Features.Labels.Commands.RemoveCardLabel;
using Harmony.Application.Features.Labels.Commands.UpdateTitle;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
{
    public class LabelsController : BaseApiController<LabelsController>
    {
        /// <summary>
        /// Add a Label
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Status 200 OK</returns>
        //[Authorize(Policy = Permissions.Products.Create)]
        //[HttpPost]
        //public async Task<IActionResult> Post(CreateLabelCommand command)
        //{
        //    return Ok(await _mediator.Send(command));
        //}

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
