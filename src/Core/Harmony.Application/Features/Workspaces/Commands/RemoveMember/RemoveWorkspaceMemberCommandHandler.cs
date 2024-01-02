using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Messaging;
using Harmony.Shared.Utilities;
using Harmony.Application.Notifications.Email;

namespace Harmony.Application.Features.Workspaces.Commands.RemoveMember
{
    /// <summary>
    /// Handler for removing a workspace member
    /// </summary>
    public class RemoveWorkspaceMemberCommandHandler : IRequestHandler<RemoveWorkspaceMemberCommand, Result<bool>>
    {
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<RemoveWorkspaceMemberCommandHandler> _localizer;

        public RemoveWorkspaceMemberCommandHandler(IUserWorkspaceRepository userWorkspaceRepository,
            ICurrentUserService currentUserService,
            IWorkspaceRepository workspaceRepository,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<RemoveWorkspaceMemberCommandHandler> localizer)
        {
            _userWorkspaceRepository = userWorkspaceRepository;
            _currentUserService = currentUserService;
            _workspaceRepository = workspaceRepository;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
        }
        public async Task<Result<bool>> Handle(RemoveWorkspaceMemberCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userWorkspace = await _userWorkspaceRepository.GetUserWorkspace(request.WorkspaceId, request.UserId);

            if (userWorkspace != null)
            {
                var dbResult = await _userWorkspaceRepository.RemoveAsync(userWorkspace);

                if (dbResult > 0)
                {
                    var workspace = await _workspaceRepository.GetAsync(request.WorkspaceId);

                    var slug = StringUtilities.SlugifyString(workspace.Name);
                    var workspaceUrl = $"/workspaces/{workspace.Id}/{slug}";

                    _notificationsPublisher
                        .PublishEmailNotification(new MemberRemovedFromWorkspaceNotification(request.WorkspaceId, request.UserId, workspaceUrl));

                    return await Result<bool>.SuccessAsync(true, _localizer["User Removed from workspace"]);
                }
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
