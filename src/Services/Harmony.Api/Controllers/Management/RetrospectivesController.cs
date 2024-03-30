using Harmony.Application.Features.Retrospectives.Commands.Create;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers.Management
{
    /// <summary>
    /// Controller for retrospective operations
    /// </summary>
    public class RetrospectivesController : BaseApiController<RetrospectivesController>
    {
        private readonly ISender _sender;

        public RetrospectivesController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRetrospectiveCommand command)
        {
            return Ok(await _sender.Send(command));
        }
    }
}
