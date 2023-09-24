using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
{
    public class CheckListsController : BaseApiController<CheckListsController>
    {

		[HttpPost]
		public async Task<IActionResult> Post(CreateChecklistCommand command)
		{
			return Ok(await _mediator.Send(command));
		}

        [HttpPost("{id:guid}/items")]
        public async Task<IActionResult> AddItem(CreateCheckListItemCommand command)
        {
            return Ok(await _mediator.Send(command));
        }
    }
}
