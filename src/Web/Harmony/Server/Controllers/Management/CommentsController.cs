using Harmony.Application.Features.Comments.Commands.CreateComment;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
{
    /// <summary>
    /// Controller for comments operations
    /// </summary>
    public class CommentsController : BaseApiController<CommentsController>
    {

        [HttpPost]
        public async Task<IActionResult> Post(CreateCommentCommand command)
        {
            return Ok(await _mediator.Send(command));
        }
    }
}
