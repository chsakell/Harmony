using Harmony.Application.Features.Boards.Commands.AddUserBoard;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.RemoveUserBoard;
using Harmony.Application.Features.Boards.Commands.UpdateUserBoardAccess;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Application.Features.Cards.Commands.UpdateCardStatus;
using Harmony.Application.Features.Lists.Commands.UpdateListsPositions;
using Harmony.Application.Features.Lists.Queries.LoadBoardList;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
{
    /// <summary>
    /// Controller for Board operations
    /// </summary>
    public class BoardController : BaseApiController<BoardController>
    {
        [HttpPost]
        public async Task<IActionResult> Post(CreateBoardCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> LoadBoard(Guid id, int size)
        {
            return Ok(await _mediator.Send(new GetBoardQuery(id, size)));
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

        [HttpDelete("{id:guid}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(Guid id, string userId)
        {
            return Ok(await _mediator.Send(new RemoveUserBoardCommand(id, userId)));
        }

        [HttpPut("{id:guid}/members/{userId}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateUserBoardAccessCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("{id:guid}/positions")]
        public async Task<IActionResult> UpdateBoardListsPositions(Guid boardId, UpdateListsPositionsCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("{id:guid}/lists/{listId:guid}")]
        public async Task<IActionResult> LoadBoardListCards(Guid id, Guid listId, int page, int maxCards)
        {
            return Ok(await _mediator.Send(new LoadBoardListQuery(id, listId, page, maxCards)));
        }
    }
}
