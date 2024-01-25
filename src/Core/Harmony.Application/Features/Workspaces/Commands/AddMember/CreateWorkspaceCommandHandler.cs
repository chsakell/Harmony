using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Messaging;
using Harmony.Shared.Utilities;
using Harmony.Application.Notifications.Email;
using Harmony.Application.Configurations;
using Microsoft.Extensions.Options;

namespace Harmony.Application.Features.Workspaces.Commands.AddMember
{
    /// <summary>
    /// Handler for adding workspace member
    /// </summary>
    public class AddWorkspaceMemberCommandHandler : IRequestHandler<AddWorkspaceMemberCommand, Result<bool>>
    {
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IOptions<AppEndpointConfiguration> _endpointsConfiguration;
        private readonly IStringLocalizer<AddWorkspaceMemberCommandHandler> _localizer;

        public AddWorkspaceMemberCommandHandler(IUserWorkspaceRepository userWorkspaceRepository,
            ICurrentUserService currentUserService,
            INotificationsPublisher notificationsPublisher,
            IWorkspaceRepository workspaceRepository,
            IOptions<AppEndpointConfiguration> endpointsConfiguration,
            IStringLocalizer<AddWorkspaceMemberCommandHandler> localizer)
        {
            _userWorkspaceRepository = userWorkspaceRepository;
            _currentUserService = currentUserService;
            _notificationsPublisher = notificationsPublisher;
            _workspaceRepository = workspaceRepository;
            _endpointsConfiguration = endpointsConfiguration;
            _localizer = localizer;
        }
        public async Task<Result<bool>> Handle(AddWorkspaceMemberCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userWorkspace = new UserWorkspace()
            {
                UserId = request.UserId,
                WorkspaceId = request.WorkspaceId,
            };

            var dbResult = await _userWorkspaceRepository.CreateAsync(userWorkspace);

            if (dbResult > 0)
            {
                var workspace = await _workspaceRepository.GetAsync(request.WorkspaceId);

                var slug = StringUtilities.SlugifyString(workspace.Name);
                var workspaceUrl = $"{_endpointsConfiguration.Value.FrontendUrl}/workspaces/{workspace.Id}/{slug}";

                _notificationsPublisher
                    .PublishEmailNotification(new MemberAddedToWorkspaceNotification(request.WorkspaceId, request.UserId, workspaceUrl));

                return await Result<bool>.SuccessAsync(true, _localizer["User added to workspace"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
