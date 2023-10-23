using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Application.Features.Lists.Commands.CreateList;
using Harmony.Application.Features.Lists.Commands.UpdateListsPositions;
using Harmony.Application.Features.Lists.Commands.UpdateListTitle;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
{
    public class BoardListsController : BaseApiController<BoardListsController>
    {
        [HttpPut("{id:guid}/title")]
        public async Task<IActionResult> Put(Guid id, UpdateListTitleCommand command)
        {
            return Ok(await _mediator.Send(command));
        }
    }
}
