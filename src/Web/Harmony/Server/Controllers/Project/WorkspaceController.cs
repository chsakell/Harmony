using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Project
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
    }
}
