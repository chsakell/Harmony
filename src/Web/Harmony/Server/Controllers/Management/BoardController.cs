using Harmony.Application.Features.Boards.Commands;

using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
{
    public class BoardController : BaseApiController<BoardController>
    {
        /// <summary>
        /// Add a Board
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Status 200 OK</returns>
        //[Authorize(Policy = Permissions.Products.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(CreateBoardCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{
        //    return Ok(await _mediator.Send(new GetUserOwnedBoardsQuery()));
        //}
    }
}
