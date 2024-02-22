using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Application.Features.Cards.Commands.DeleteChecklist;
using Harmony.Application.Features.Lists.Commands.UpdateCheckListTitle;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers.Management
{
    /// <summary>
    /// Controller for Check Lists operations
    /// </summary>
    public class CheckListsController : BaseApiController<CheckListsController>
    {

        [HttpPost]
        public async Task<IActionResult> Post(CreateCheckListCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPost("{id:guid}/items")]
        public async Task<IActionResult> AddItem(CreateCheckListItemCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("{id:guid}/title")]
        public async Task<IActionResult> UpdateTitle(Guid id, UpdateCheckListTitleCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await _mediator.Send(new DeleteCheckListCommand(id)));
        }
    }
}
