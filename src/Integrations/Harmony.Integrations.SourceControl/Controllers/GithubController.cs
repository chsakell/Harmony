using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.SourceControl.Commands.CreateBranch;
using Harmony.Application.Features.SourceControl.Commands.DeleteBranch;
using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Integrations.SourceControl.Constants;
using Harmony.Integrations.SourceControl.Models;
using Harmony.Integrations.SourceControl.WebhookRequests;
using Harmony.Integrations.SourceControl.WebhookRequests.GitHub;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.Json;

namespace Harmony.Integrations.SourceControl.Controllers
{
    public class GithubController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GithubController> _logger;

        public GithubController(
            IMediator mediator,
            ILogger<GithubController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var eventType = Request.Headers[GithubConstants.GitHubEventHeader];

            if (eventType == "ping")
            {
                return Ok();
            }

            string postData = null;
            // Read the post data from the request body
            using (var reader = new StreamReader(HttpContext.Request.Body))
            {
                postData = await reader.ReadToEndAsync();

            }

            if(eventType == "push")
            {
                var request = JsonSerializer.Deserialize<GitHubPushRequest>(postData);
            }
            else if (eventType == "branch")
            {
                var request = JsonSerializer.Deserialize<GitHubBranchRequest>(postData);

                if (request.ref_type.Equals("branch"))
                {
                    switch (eventType)
                    {
                        case "create":
                            await _mediator.Send(new CreateBranchCommand()
                            {
                                Name = request.Ref,
                                Provider = SourceControlProvider.GitHub,
                                RepositoryId = request.repository.id.ToString(),
                                RepositoryName = request.repository.name,
                                RepositoryFullName = request.repository.full_name,
                                RepositoryUrl = request.repository.html_url,
                            });
                            break;
                        case "delete":
                            await _mediator.Send(new DeleteBranchCommand()
                            {
                                Name = request.Ref,
                                RepositoryId = request.repository.id.ToString()
                            });
                            break;
                        default:
                            break;
                    }
                }
            }

            return Ok();
        }
    }
}
