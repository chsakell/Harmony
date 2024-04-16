using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
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
            var push = new Push()
            {
                Id = Guid.NewGuid().ToString(),
                Commits = request.Commits,
                CompareUrl = request.CompareUrl,
                PusherEmail = request.PusherEmail,
                PusherName = request.PusherName,
                Ref = request.Ref,
                RepositoryId = request.RepositoryId,
                SenderAvatarUrl = request.SenderAvatarUrl,
                SenderId = request.SenderId,
                SenderLogin = request.SenderLogin
            };

            await _sourceControlRepository.CreatePush(push);

            return Result<bool>.Success(true, _localizer["Push created"]);
        }
    }
}
