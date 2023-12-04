using Harmony.Application.Features.Labels.Commands.CreateCardLabel;
using Harmony.Application.Features.Labels.Commands.RemoveCardLabel;
using Harmony.Application.Features.Labels.Commands.UpdateTitle;
using Harmony.Application.Features.Users.Queries.GetUserNotifications;
using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
{
    /// <summary>
    /// Controller for user notifications
    /// </summary>
    public class UserNotificationsController : BaseApiController<UserNotificationsController>
    {
        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(string userId)
        {
            return Ok(await _mediator.Send(new GetUserNotificationsQuery(userId)));
        }
    }
}
