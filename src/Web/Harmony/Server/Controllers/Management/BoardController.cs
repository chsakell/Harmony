using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetAllForUser;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
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

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _mediator.Send(new GetAllForUserBoardsQuery()));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> LoadBoard(Guid id)
        {
            return Ok(await _mediator.Send(new GetBoardQuery(id)));
        }

		[HttpPost("{id:guid}/lists")]
		public async Task<IActionResult> CreateList(CreateListCommand command)
		{
			return Ok(await _mediator.Send(command));
		}
	}
}
