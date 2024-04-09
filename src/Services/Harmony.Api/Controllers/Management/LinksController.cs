using Harmony.Application.Features.Cards.Commands.DeleteLink;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers.Management
{
    public class LinksController : BaseApiController<LinksController>
    {
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteLink(Guid id, [FromQuery] Guid boardId)
        {
            var command = new DeleteLinkCommand(boardId, id);

            return Ok(await _mediator.Send(command));
        }
    }
}
