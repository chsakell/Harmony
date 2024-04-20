using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.SourceControl.Commands.GetOrCreateRepository;
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
            var pullRequesst = new PullRequest()
            {
                ClosedAt = request.ClosedAt,
                CreatedAt = request.CreatedAt,
                DiffUrl = request.DiffUrl,
                HtmlUrl = request.HtmlUrl,
                Id = Guid.NewGuid().ToString(),
                MergedAt = request.MergedAt,
                Number = request.Number,
                Sender = request.Sender,
                Assignees = request.Assignees,
                State = request.State,
                SourceBranch = request.SourceBranch,
                TargetBranch = request.TargetBranch,
                Title = request.Title,
                UpdatedAt = request.UpdatedAt
            };

            return Result<bool>.Success(true, _localizer["Pull request created"]);
        }
    }
}
