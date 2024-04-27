using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.SourceControl.Commands.CreateBranch;
using Harmony.Application.Features.SourceControl.Commands.GetOrCreateRepository;
using Harmony.Application.SourceControl.Messages;
using Harmony.Domain.Enums;
using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePullRequest
{
    public class CreatePullRequestCommandHandler : IRequestHandler<CreatePullRequestCommand, Result<bool>>
    {
        private readonly ISourceControlRepository _sourceControlRepository;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IMediator _mediator;
        private readonly IStringLocalizer<CreatePullRequestCommandHandler> _localizer;

        public CreatePullRequestCommandHandler(ISourceControlRepository sourceControlRepository,
            INotificationsPublisher notificationsPublisher,
            IMediator mediator,
            IStringLocalizer<CreatePullRequestCommandHandler> localizer)
        {
            _sourceControlRepository = sourceControlRepository;
            _notificationsPublisher = notificationsPublisher;
            _mediator = mediator;
            _localizer = localizer;
        }

        public async Task<Result<bool>> Handle(CreatePullRequestCommand request, CancellationToken cancellationToken)
        {
            await _mediator.Send(new GetOrCreateRepositoryCommand()
            {
                RepositoryId = request.Repository.RepositoryId,
                Url = request.Repository.Url,
                Name = request.Repository.Name,
                FullName = request.Repository.FullName,
                Provider = request.Repository.Provider
            });


            await _mediator.Send(new CreateBranchCommand()
            {
                Name = request.SourceBranch,
                Repository = request.Repository,
                Creator = request.Sender,
                SkipRepositoryCheck = true
            });


            var pullRequest = new PullRequest()
            {
                Id = request.Id.ToString(),
                HtmlUrl = request.HtmlUrl,
                DiffUrl = request.DiffUrl,
                State = request.State,
                Title = request.Title,
                Number = request.Number,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                ClosedAt = request.ClosedAt,
                MergedAt = request.MergedAt,
                SourceBranch = request.SourceBranch,
                TargetBranch = request.TargetBranch,
                MergeCommitSha = request.MergeCommitSha,
                Assignees = request.Assignees,
                Reviewers = request.Reviewers,
            };

            await _sourceControlRepository
                .AddOrUpdatePullRequest(request.Repository.RepositoryId, pullRequest);

            if (!string.IsNullOrEmpty(request.SerialKey))
            {
                var message = new BranchPullRequestCreatedMessage()
                {
                    SerialKey = request.SerialKey.ToLower(),
                    PullRequest = new DTO.PullRequestDto()
                    {
                        Assignees = request.Assignees.Select(a => new DTO.RepositoryUserDto()
                        {
                            AvatarUrl = a.AvatarUrl,
                            HtmlUrl = a.HtmlUrl,
                            Login = a.Login,
                        }).ToList(),
                        ClosedAt = request.ClosedAt,
                        MergedAt = request.MergedAt,
                        SourceBranch = request.SourceBranch,
                        TargetBranch = request.TargetBranch,
                        MergeCommitSha = request.MergeCommitSha,
                        Number = request.Number,
                        CreatedAt = request.CreatedAt,
                        DiffUrl = request.DiffUrl,
                        HtmlUrl = request.HtmlUrl,
                        Id = request.Id,
                        Reviewers = request.Reviewers.Select(a => new DTO.RepositoryUserDto()
                        {
                            AvatarUrl = a.AvatarUrl,
                            HtmlUrl = a.HtmlUrl,
                            Login = a.Login,
                        }).ToList(),
                        State = request.State,
                        Title = request.Title,
                        UpdatedAt = request.UpdatedAt
                    },
                    Sender = new DTO.RepositoryUserDto()
                    {
                        AvatarUrl = request.Sender.AvatarUrl,
                        HtmlUrl = request.Sender.HtmlUrl,
                        Login = request.Sender.Login
                    }
                };

                _notificationsPublisher.PublishMessage(message,
                    NotificationType.BranchPullRequestCreated, routingKey: BrokerConstants.RoutingKeys.SignalR);
            }

            return Result<bool>.Success(true, _localizer["Pull request created"]);
        }
    }
}
