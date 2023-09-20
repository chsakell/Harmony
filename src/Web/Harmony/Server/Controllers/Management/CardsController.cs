using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.CreateCard;
using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetAllForUser;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
{
    public class CardsController : BaseApiController<CardsController>
    {
        /// <summary>
        /// Add a Board
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Status 200 OK</returns>
        //[Authorize(Policy = Permissions.Products.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(CreateCardCommand command)
        {
            return Ok(await _mediator.Send(command));
        }
	}
}
