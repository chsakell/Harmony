using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;

namespace Harmony.Application.Features.Workspaces.Commands.RemoveMember
{
    public class RemoveWorkspaceMemberCommandHandler : IRequestHandler<RemoveWorkspaceMemberCommand, Result<bool>>
    {
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<RemoveWorkspaceMemberCommandHandler> _localizer;

        public RemoveWorkspaceMemberCommandHandler(IUserWorkspaceRepository userWorkspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<RemoveWorkspaceMemberCommandHandler> localizer)
        {
            _userWorkspaceRepository = userWorkspaceRepository;
            _currentUserService = currentUserService;
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
                    return await Result<bool>.SuccessAsync(true, _localizer["User Removed from workspace"]);
                }
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
