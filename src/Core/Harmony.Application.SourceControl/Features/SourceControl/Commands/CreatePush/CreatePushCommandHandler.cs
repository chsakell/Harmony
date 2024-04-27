using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.SourceControl.Commands.CreateBranch;
using Harmony.Application.Features.SourceControl.Commands.GetOrCreateRepository;
using Harmony.Application.SourceControl.Messages;
using Harmony.Domain.Enums;
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

            if (!string.IsNullOrEmpty(request.SerialKey) && request.Commits.Any())
            {
                var message = new BranchCommitsPushedMessage()
                {
                    SerialKey = request.SerialKey.ToLower(),
                    Branch = request.Branch,
                    Commits = request.Commits.Select(c => new DTO.CommitDto()
                    {
                        Id = c.Id,
                        Added = c.Added,
                        Author = new DTO.AuthorDto()
                        {
                            Email = c.Author.Email,
                            Name = c.Author.Name,
                            Username = c.Author.Username,
                        },
                        Message = c.Message,
                        Modified = c.Modified,
                        Removed = c.Removed,
                        Timestamp = c.Timestamp,
                        Url = c.Url
                    }).ToList()
                };

                _notificationsPublisher.PublishMessage(message,
                    NotificationType.BranchCommitsPushed, routingKey: BrokerConstants.RoutingKeys.SignalR);
            }

            return Result<bool>.Success(true, _localizer["Push created"]);
        }
    }
}
