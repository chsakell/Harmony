using Harmony.Application.Features.Boards.Commands.AddUserBoard;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> LoadBoard(Guid id)
        {
            return Ok(await _mediator.Send(new GetBoardQuery(id)));
        }

        [HttpGet("{id:guid}/members")]
        public async Task<IActionResult> GetMembers(Guid id)
        {
            return Ok(await _mediator.Send(new GetBoardUsersQuery(id)));
        }

        [HttpGet("{id:guid}/members/search")]
        public async Task<IActionResult> SearchMembers(Guid id, string term)
        {
            return Ok(await _mediator.Send(new SearchBoardUsersQuery(id, term)));
        }

        [HttpPost("{id:guid}/members")]
        public async Task<IActionResult> AddUserToBoard(Guid id, AddUserBoardCommand command)
        {
            return Ok(await _mediator.Send(command));
        }
    }
}
