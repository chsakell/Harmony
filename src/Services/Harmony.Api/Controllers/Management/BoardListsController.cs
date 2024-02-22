using Harmony.Application.Features.Lists.Commands.UpdateListTitle;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers.Management
{
    /// <summary>
    /// Controller for Board List operations
    /// </summary>
    public class BoardListsController : BaseApiController<BoardListsController>
    {
        [HttpPut("{id:guid}/title")]
        public async Task<IActionResult> Put(Guid id, UpdateListTitleCommand command)
        {
            return Ok(await _mediator.Send(command));
        }
    }
}
