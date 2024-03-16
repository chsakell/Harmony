using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Features.Workspaces.Commands.Update;

namespace Harmony.Application.Features.Workspaces.Commands.Rename
{
    /// <summary>
    /// Handler for creating workspace
    /// </summary>
    public class RenameWorkspaceCommandHandler : IRequestHandler<RenameWorkspaceStatusCommand, Result<bool>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<UpdateWorkspaceCommandHandler> _localizer;

        public RenameWorkspaceCommandHandler(IWorkspaceRepository workspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<UpdateWorkspaceCommandHandler> localizer)
        {
            _workspaceRepository = workspaceRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }
        public async Task<Result<bool>> Handle(RenameWorkspaceStatusCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var workspace = await _workspaceRepository.GetAsync(request.Id);

            if (workspace == null)
            {
                return await Result<bool>.FailAsync(_localizer["Workspace not found"]);
            }

            workspace.Name = request.Name;

            var dbResult = await _workspaceRepository.Update(workspace);

            if (dbResult > 0)
            {
                return await Result<bool>.SuccessAsync(true, _localizer["Workspace renamed"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
