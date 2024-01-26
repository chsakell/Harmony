using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Microsoft.AspNetCore.Mvc;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Application.Features.Workspaces.Commands.AddMember;
using Harmony.Application.Features.Workspaces.Commands.RemoveMember;
using Harmony.Application.Features.Workspaces.Queries.SearchWorkspaceUsers;

namespace Harmony.Api.Controllers.Management
{
    /// <summary>
    /// Controller for Workspace operations
    /// </summary>
    public class WorkspaceController : BaseApiController<WorkspaceController>
    {
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

        [HttpGet("{id:guid}/members/search")]
        public async Task<IActionResult> SearchMembers(Guid id, string term)
        {
            return Ok(await _mediator.Send(new SearchWorkspaceUsersQuery(id, term)));
        }

        [HttpPost("{id:guid}/members/add")]
        public async Task<IActionResult> AddMember(AddWorkspaceMemberCommand request)
        {
            return Ok(await _mediator.Send(request));
        }

        [HttpPost("{id:guid}/members/remove")]
        public async Task<IActionResult> DeleteMember(RemoveWorkspaceMemberCommand request)
        {
            return Ok(await _mediator.Send(request));
        }
    }
}
