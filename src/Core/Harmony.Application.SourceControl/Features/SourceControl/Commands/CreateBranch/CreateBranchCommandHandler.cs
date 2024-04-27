using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.SourceControl.Commands.GetOrCreateRepository;
using Harmony.Application.SourceControl.Messages;
using Harmony.Domain.Enums;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.SourceControl.Commands.CreateBranch
{
    public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Result<bool>>
    {
        private readonly ISourceControlRepository _sourceControlRepository;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IMediator _mediator;
        private readonly IStringLocalizer<CreateBranchCommandHandler> _localizer;

        public CreateBranchCommandHandler(ISourceControlRepository sourceControlRepository,
            INotificationsPublisher notificationsPublisher,
            IMediator mediator,
            IStringLocalizer<CreateBranchCommandHandler> localizer)
        {
            _sourceControlRepository = sourceControlRepository;
            _notificationsPublisher = notificationsPublisher;
            _mediator = mediator;
            _localizer = localizer;
        }

        public async Task<Result<bool>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
        {
            Branch branch = await _sourceControlRepository
                .GetBranch(request.Name, request.Repository.RepositoryId);

            if (branch != null)
            {
                return await Result<bool>.FailAsync(_localizer["Branch already exists"]);
            }

            if (!request.SkipRepositoryCheck)
            {
                await _mediator.Send(new GetOrCreateRepositoryCommand()
                {
                    RepositoryId = request.Repository.RepositoryId,
                    Url = request.Repository.Url,
                    Name = request.Repository.Name,
                    FullName = request.Repository.FullName,
                    Provider = request.Repository.Provider
                });
            }

            branch = new Branch()
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Creator = request.Creator,
                RepositoryId = request.Repository.RepositoryId,
                Commits = new List<Commit>(),
                PullRequests = new List<PullRequest>()
            };

            await _sourceControlRepository.CreateBranch(branch);

            if (!string.IsNullOrEmpty(request.SerialKey))
            {
                var message = new BranchCreatedMessage()
                {
                    SerialKey = request.SerialKey.ToLower(),
                    Id = branch.Id,
                    Name = branch.Name,
                    Creator = new Application.SourceControl.DTO.RepositoryUserDto()
                    {
                        Login = branch.Creator.Login,
                        AvatarUrl = branch.Creator.AvatarUrl,
                        HtmlUrl = branch.Creator.HtmlUrl,
                    },
                    Provider = request.Repository.Provider,
                    RepositoryName = request.Repository.Name,
                    RepositoryUrl = request.Repository.Url,
                };

                _notificationsPublisher.PublishMessage(message,
                    NotificationType.BranchCreated, routingKey: BrokerConstants.RoutingKeys.SignalR);
            }

            return Result<bool>.Success(true, _localizer["Branch created"]);
        }
    }
}
