using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.SourceControl.Commands.CreateBranch;
using Harmony.Application.Features.SourceControl.Commands.GetOrCreateRepository;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePush
{
    public class CreatePushCommandHandler : IRequestHandler<CreatePushCommand, Result<bool>>
    {
        private readonly ISourceControlRepository _sourceControlRepository;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IMediator _mediator;
        private readonly IStringLocalizer<CreatePushCommandHandler> _localizer;

        public CreatePushCommandHandler(ISourceControlRepository sourceControlRepository,
            INotificationsPublisher notificationsPublisher,
            IMediator mediator,
            IStringLocalizer<CreatePushCommandHandler> localizer)
        {
            _sourceControlRepository = sourceControlRepository;
            _notificationsPublisher = notificationsPublisher;
            _mediator = mediator;
            _localizer = localizer;
        }

        public async Task<Result<bool>> Handle(CreatePushCommand request, CancellationToken cancellationToken)
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
                Name = request.Branch,
                Repository = request.Repository,
                Creator = request.Sender,
                SkipRepositoryCheck = true
            });

            await _sourceControlRepository.CreatePush(request.Repository.RepositoryId, request.Branch, request.Commits);

            return Result<bool>.Success(true, _localizer["Push created"]);
        }
    }
}
