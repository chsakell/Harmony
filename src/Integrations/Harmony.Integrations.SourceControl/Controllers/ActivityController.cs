using Harmony.Application.Features.SourceControl.Commands.CreateBranch;
using Harmony.Application.Features.SourceControl.Commands.DeleteBranch;
using Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePullRequest;
using Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePush;
using Harmony.Application.SourceControl.Features.SourceControl.Queries.GetCardBranches;
using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Integrations.SourceControl.Constants;
using Harmony.Integrations.SourceControl.WebhookRequests.GitHub;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Harmony.Integrations.SourceControl.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GithubController> _logger;

        public ActivityController(
            IMediator mediator,
            ILogger<GithubController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("branches")]
        public async Task<IActionResult> GetCardBranches([FromQuery] string serialKey)
        {
            return Ok(await _mediator.Send(new GetCardBranchesQuery(serialKey)));
        }
    }
}
