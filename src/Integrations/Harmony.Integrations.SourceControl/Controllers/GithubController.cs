using Harmony.Integrations.SourceControl.Constants;
using Harmony.Integrations.SourceControl.Models;
using Harmony.Integrations.SourceControl.WebhookRequests;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Harmony.Integrations.SourceControl.Controllers
{
    public class GithubController : Controller
    {
        private readonly ILogger<GithubController> _logger;

        public GithubController(ILogger<GithubController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Index([FromBody] GithubWebhookRequest request)
        {
            var eventType = Request.Headers[GithubConstants.GithubEventHeader];

            return Ok();
        }
    }
}
