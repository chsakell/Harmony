using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.SourceControl.Commands.GetOrCreateRepository;
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

            var pullRequest = new PullRequest()
            {
                Id = request.Id.ToString(),
                //Action = request.Action,
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

            return Result<bool>.Success(true, _localizer["Pull request created"]);
        }
    }
}
