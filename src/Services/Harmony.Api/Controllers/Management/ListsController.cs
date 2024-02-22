using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Application.Features.Lists.Commands.CreateList;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers.Management
{
    /// <summary>
    /// Controller for List operations
    /// </summary>
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
