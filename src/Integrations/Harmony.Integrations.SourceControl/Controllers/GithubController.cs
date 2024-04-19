using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.Cards.Queries.GetActivity;
using Harmony.Application.Features.SourceControl.Commands.CreateBranch;
using Harmony.Application.Features.SourceControl.Commands.DeleteBranch;
using Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePullRequest;
using Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePush;
using Harmony.Application.SourceControl.Features.SourceControl.Queries.GetCardBranches;
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

        /// <summary>
        /// Github webhook
        /// </summary>
        /// <returns></returns>
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

            if (eventType == "push")
            {
                var request = JsonSerializer.Deserialize<GitHubPushRequest>(postData);

                if (request.created || request.deleted)
                {
                    return Ok();
                }

                var push = new CreatePushCommand()
                {
                    Commits = request.commits.Select(commit =>
                        new Domain.SourceControl.Commit()
                        {
                            Added = commit.added.Select(a => a.ToString()).ToList(),
                            CommiterEmail = commit.committer.email,
                            CommiterName = commit.committer.name,
                            CommiterUsername = commit.committer.username,
                            Id = commit.id,
                            Message = commit.message,
                            Modified = commit.modified.Select(a => a.ToString()).ToList(),
                            Removed = commit.removed.Select(a => a.ToString()).ToList(),
                            Timestamp = commit.timestamp,
                            Url = commit.url
                        }).ToList(),
                    CompareUrl = request.compare,
                    PusherEmail = request.pusher.email,
                    PusherName = request.pusher.name,
                    Ref = request.Ref,
                    RepositoryId = request.repository.id.ToString(),
                    SenderAvatarUrl = request.sender.avatar_url,
                    SenderId = request.sender.id.ToString(),
                    SenderLogin = request.sender.login,
                };

                await _mediator.Send(push);
            }
            else if (eventType == "create" || eventType == "delete")
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
            else if(eventType == "pull_request")
            {
                var request = JsonSerializer.Deserialize<GitHubPullRequest>(postData);

                if(request != null)
                {
                    var pullRequest = new CreatePullRequestCommand()
                    {
                        Action = request.action,
                        ClosedAt = request.pull_request.closed_at,
                        CreatedAt = request.pull_request.created_at,
                        DiffUrl = request.pull_request.diff_url,
                        HtmlUrl = request.pull_request.html_url,
                        MergedAt = request.pull_request.merged_at,
                        Number = request.number,
                        PullRequestId = request.pull_request.id.ToString(),
                        SenderAvatarUrl = request.sender.avatar_url,
                        SenderId = request.sender.id.ToString(),
                        SenderLogin = request.sender.login,
                        State = request.pull_request.state,
                        SourceBranch = request.pull_request.head.Ref,
                        TargetBranch = request.pull_request.Base.Ref,
                        Title = request.pull_request.title,
                        UpdatedAt = request.pull_request.updated_at
                    };

                    await _mediator.Send(pullRequest);
                }
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetCardBranches([FromQuery] string serialKey)
        {
            return Ok(await _mediator.Send(new GetCardBranchesQuery(serialKey)));
        }
    }
}
