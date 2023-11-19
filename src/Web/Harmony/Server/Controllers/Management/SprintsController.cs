using Harmony.Application.Features.Sprints.StartSprint;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
{
    /// <summary>
    /// Controller for Sprint operations
    /// </summary>
    public class SprintsController : BaseApiController<SprintsController>
    {
        private readonly ISender _sender;

        public SprintsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPut("{id:guid}/start")]
        public async Task<IActionResult> Put(Guid id, StartSprintCommand command)
        {
            return Ok(await _sender.Send(command));
        }
    }
}
