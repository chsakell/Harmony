using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Microsoft.AspNetCore.Mvc;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;

namespace Harmony.Server.Controllers.Management
{
    public class WorkspaceController : BaseApiController<WorkspaceController>
    {
        /// <summary>
        /// Add a Workspace
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Status 200 OK</returns>
        //[Authorize(Policy = Permissions.Products.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(CreateWorkspaceCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _mediator.Send(new GetUserWorkspacesQuery()));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _mediator.Send(new LoadWorkspaceQuery(id)));
        }

        [HttpGet("{id:guid}/boards")]
        public async Task<IActionResult> GetBoards(Guid id)
        {
            return Ok(await _mediator.Send(new GetWorkspaceBoardsQuery(id)));
        }
        

        [HttpGet("{id:guid}/members")]
        public async Task<IActionResult> GetMembers(Guid id, int pageNumber, int pageSize,
            string searchTerm = null, string orderBy = null, bool membersOnly = false)
        {
            return Ok(await _mediator.Send(new 
                GetWorkspaceUsersQuery(id, pageNumber, pageSize, searchTerm, orderBy, membersOnly)));
        }
    }
}
