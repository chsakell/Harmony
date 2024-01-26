using Harmony.Application.Features.Lists.Commands.UpdateListItemChecked;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDescription;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDueDate;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers.Management
{
    /// <summary>
    /// Controller for Check List items
    /// </summary>
    public class CheckListItemsController : BaseApiController<CheckListItemsController>
    {

        [HttpPut("{id:guid}/description")]
        public async Task<IActionResult> UpdateDescription(Guid id, UpdateListItemDescriptionCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("{id:guid}/checked")]
        public async Task<IActionResult> UpdateDescription(Guid id, UpdateListItemCheckedCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("{id:guid}/duedate")]
        public async Task<IActionResult> UpdateDueDate(Guid id, UpdateListItemDueDateCommand command)
        {
            return Ok(await _mediator.Send(command));
        }
    }
}
