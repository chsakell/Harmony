using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
{
    public class ListsController : BaseApiController<ListsController>
    {

		[HttpPost]
		public async Task<IActionResult> Post(CreateListCommand command)
		{
			return Ok(await _mediator.Send(command));
		}

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> Put(Guid id, UpdateListStatusCommand command)
        {
            return Ok(await _mediator.Send(command));
        }
    }
}
