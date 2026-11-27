using Harmony.Application.Features.Users.Commands.UpdateUserNotifications;
using Harmony.Application.Features.Users.Queries.GetUserNotifications;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers.Management
{
    /// <summary>
    /// Controller for user notifications
    /// </summary>
    public class UserNotificationsController : BaseApiController<UserNotificationsController>
    {
        /// <summary>The logged-in user's own notification settings.</summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _mediator.Send(new GetUserNotificationsQuery()));
        }

        [HttpPost]
        public async Task<IActionResult> Post(UpdateUserNotificationsCommand command)
        {
            return Ok(await _mediator.Send(command));
        }
    }
}
