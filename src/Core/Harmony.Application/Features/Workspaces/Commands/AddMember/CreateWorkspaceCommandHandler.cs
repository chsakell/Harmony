using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;

namespace Harmony.Application.Features.Workspaces.Commands.AddMember
{
    /// <summary>
    /// Handler for adding workspace member
    /// </summary>
    public class AddWorkspaceMemberCommandHandler : IRequestHandler<AddWorkspaceMemberCommand, Result<bool>>
    {
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<AddWorkspaceMemberCommandHandler> _localizer;

        public AddWorkspaceMemberCommandHandler(IUserWorkspaceRepository userWorkspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<AddWorkspaceMemberCommandHandler> localizer)
        {
            _userWorkspaceRepository = userWorkspaceRepository;
            _currentUserService = currentUserService;
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
                return await Result<bool>.SuccessAsync(true, _localizer["User added to workspace"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
