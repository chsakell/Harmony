using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Application.Features.Cards.Commands.UpdateCardTitle;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Application.Features.Lists.Commands.UpdateListItemChecked;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDescription;
using Harmony.Application.Features.Lists.Commands.UpdateListTitle;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
{
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
    }
}
