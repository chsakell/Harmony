using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Integrations.SourceControl.Constants;
using Harmony.Integrations.SourceControl.Models;
using Harmony.Integrations.SourceControl.WebhookRequests;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Harmony.Integrations.SourceControl.Controllers
{
    public class GithubController : Controller
    {
        private readonly ISourceControlRepository _sourceControlRepository;
        private readonly ILogger<GithubController> _logger;

        public GithubController(
            ISourceControlRepository sourceControlRepository,
            ILogger<GithubController> logger)
        {
            _sourceControlRepository = sourceControlRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] GithubWebhookRequest request)
        {
            if(request == null)
            {
                return Ok();
            }

            var eventType = Request.Headers[GithubConstants.GitHubEventHeader];

            if(request.ref_type.Equals("branch"))
            {
                switch(eventType)
                {
                    case "create":
                        var branch = new Branch()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = request.Ref,
                            RepositoryUrl = request.repository.html_url,
                            Provider = SourceControlProvider.GitHub
                        };

                        await _sourceControlRepository.CreateBranch(branch);
                        break;
                    case "delete":
                        await _sourceControlRepository.DeleteBranch(request.Ref);
                        break;
                    default:
                        break;
                }
            }

            return Ok();
        }
    }
}
