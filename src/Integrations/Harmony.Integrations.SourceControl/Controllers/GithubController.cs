using Harmony.Application.Features.SourceControl.Commands.CreateBranch;
using Harmony.Application.Features.SourceControl.Commands.DeleteBranch;
using Harmony.Application.Helpers;
using Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePullRequest;
using Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePush;
using Harmony.Application.SourceControl.Features.SourceControl.Queries.GetCardBranches;
using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Integrations.SourceControl.Constants;
using Harmony.Integrations.SourceControl.WebhookRequests.GitHub;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace Harmony.Integrations.SourceControl.Controllers
{
    public class GithubController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GithubController> _logger;
        private static int MAX_CONCURRENCY = 1;
        private static int SEMAPHORE_TIMEOUT_MIL = 3000;

        private static ConcurrentDictionary<string, SemaphoreSlim> _semaphores =
                new ConcurrentDictionary<string, SemaphoreSlim>();
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

            switch (eventType)
            {
                case "push":
                    await HandlePushWebhook(postData);
                    break;
                case "create":
                case "delete":
                    await HandleEventWebhook(postData, eventType);
                    break;
                case "pull_request":
                    await HandlePullRequestWebhook(postData);
                    break;
                default:
                    break;
            }

            return Ok();
        }

        private async Task HandleEventWebhook(string postData, string eventType)
        {
            var request = JsonSerializer.Deserialize<GitHubBranchRequest>(postData);

            if (request.Type.Equals("branch"))
            {
                switch (eventType)
                {
                    case "create":
                        var serialKey = CardHelper.GetSerialKey(request.Ref);

                        await RunWithLock(serialKey, nameof(HandleEventWebhook), async () =>
                        {
                            await _mediator.Send(new CreateBranchCommand()
                            {
                                SerialKey = serialKey,
                                Name = request.Ref,
                                Repository = new Repository()
                                {
                                    RepositoryId = request.Repository.Id.ToString(),
                                    Name = request.Repository.Name,
                                    FullName = request.Repository.FullName,
                                    Url = request.Repository.HtmlUrl,
                                    Provider = SourceControlProvider.GitHub
                                },
                                Creator = new RepositoryUser(request.Sender.Login, request.Sender.AvatarUrl, request.Sender.HtmlUrl)
                            });
                        });
                        break;
                    case "delete":
                        await _mediator.Send(new DeleteBranchCommand()
                        {
                            Name = request.Ref,
                            RepositoryId = request.Repository.Id.ToString()
                        });
                        break;
                    default:
                        break;
                }
            }
        }


        private async Task HandlePushWebhook(string postData)
        {
            var request = JsonSerializer.Deserialize<GitHubPushRequest>(postData);
            var tagPushed = false;
            var branchPushed = false;
            string pushedRef = string.Empty;

            if(request.Ref.Contains("refs/heads/"))
            {
                pushedRef = request.Ref.Replace("refs/heads/", string.Empty);
                branchPushed = true;
            }
            else if(request.Ref.Contains("refs/tags/"))
            {
                pushedRef = request.Ref.Replace("refs/tags/", string.Empty);
                tagPushed = true;
            }
            else
            {
                return;
            }

            var serialKey = CardHelper.GetSerialKey(pushedRef);

            await RunWithLock(serialKey, nameof(HandlePushWebhook), async () =>
            {
                var push = new CreatePushCommand()
                {
                    SerialKey = serialKey,
                    Ref = pushedRef,
                    Repository = new Repository()
                    {
                        RepositoryId = request.Repository.Id.ToString(),
                        Name = request.Repository.Name,
                        FullName = request.Repository.FullName,
                        Url = request.Repository.HtmlUrl,
                        Provider = SourceControlProvider.GitHub
                    },
                    Commits = request.Commits.Select(commit =>
                        new Commit()
                        {
                            Url = commit.Url,
                            Author = new Author()
                            {
                                Email = commit.Author.Email,
                                Name = commit.Author.Name,
                                Username = commit.Author.Username,
                            },
                            Id = commit.Id,
                            Message = commit.Message,
                            Timestamp = commit.Timestamp,
                            Added = commit.Added ?? new List<string>(),
                            Removed = commit.Removed ?? new List<string>(),
                            Modified = commit.Modified ?? new List<string>(),
                        }).ToList(),
                    Sender = new RepositoryUser(request.Sender.Login, request.Sender.AvatarUrl, request.Sender.HtmlUrl),
                    HeadCommit = new Commit()
                    {
                        Url = request.HeadCommit.Url,
                        Author = new Author()
                        {
                            Email = request.HeadCommit.Author.Email,
                            Name = request.HeadCommit.Author.Name,
                            Username = request.HeadCommit.Author.Username,
                        },
                        Id = request.HeadCommit.Id,
                        Message = request.HeadCommit.Message,
                        Timestamp = request.HeadCommit.Timestamp,
                        Added = request.HeadCommit.Added ?? new List<string>(),
                        Removed = request.HeadCommit.Removed ?? new List<string>(),
                        Modified = request.HeadCommit.Modified ?? new List<string>(),
                    },
                    BranchPushed = branchPushed,
                    TagPushed = tagPushed
                };

                await _mediator.Send(push);
            });
        }

        private async Task HandlePullRequestWebhook(string postData)
        {
            var request = JsonSerializer.Deserialize<GitHubPullRequest>(postData);

            if (request != null)
            {
                var serialKey = CardHelper.GetSerialKey(request.PullRequest.Head.Ref);

                await RunWithLock(serialKey, nameof(HandlePullRequestWebhook), async () =>
                {
                    var pullRequest = new CreatePullRequestCommand()
                    {
                        SerialKey = serialKey,
                        Id = request.PullRequest.Id.ToString(),
                        Action = request.Action,
                        HtmlUrl = request.PullRequest.HtmlUrl,
                        DiffUrl = request.PullRequest.DiffUrl,
                        State = request.PullRequest.State
                    switch
                        {
                            "opened" => PullRequestState.Opened,
                            "closed" => request.PullRequest.MergedAt.HasValue ? PullRequestState.Merged : PullRequestState.Closed,
                            _ => PullRequestState.Opened
                        },
                        Title = request.PullRequest.Title,
                        Number = request.PullRequest.Number,
                        CreatedAt = request.PullRequest.CreatedAt,
                        UpdatedAt = request.PullRequest.UpdatedAt,
                        ClosedAt = request.PullRequest.ClosedAt,
                        MergedAt = request.PullRequest.MergedAt,
                        SourceBranch = request.PullRequest.Head.Ref,
                        TargetBranch = request.PullRequest.Base.Ref,
                        MergeCommitSha = request.PullRequest.MergeCommitSha,
                        Assignees = request.PullRequest.Assignees.Select(a =>
                            new RepositoryUser(a.Login, a.AvatarUrl, a.HtmlUrl)).ToList(),
                        Reviewers = request.PullRequest.RequestedReviewers.Select(a =>
                            new RepositoryUser(a.Login, a.AvatarUrl, a.HtmlUrl)).ToList(),
                        Repository = new Repository()
                        {
                            RepositoryId = request.Repository.Id.ToString(),
                            Name = request.Repository.Name,
                            FullName = request.Repository.FullName,
                            Url = request.Repository.HtmlUrl,
                            Provider = SourceControlProvider.GitHub
                        },
                        Sender = new RepositoryUser(request.Sender.Login, request.Sender.AvatarUrl, request.Sender.HtmlUrl)
                    };

                    await _mediator.Send(pullRequest);
                });
            }
        }

        private async Task RunWithLock(string serialKey, string funcName, Func<Task> func)
        {
            SemaphoreSlim semaphore = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(serialKey))
                {
                    semaphore = new SemaphoreSlim(MAX_CONCURRENCY);
                    semaphore = _semaphores.GetOrAdd(serialKey, semaphore);
                    var accessGranted = await semaphore.WaitAsync(SEMAPHORE_TIMEOUT_MIL);

                    if (!accessGranted)
                    {
                        _logger.LogWarning($"Couldn't acquire access for {funcName}");
                    }
                }

                await func();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling {funcName}");
            }
            finally
            {
                if (semaphore != null)
                {
                    semaphore.Release();
                    _semaphores.TryRemove(serialKey, out _);
                }
            }
        }
    }
}
