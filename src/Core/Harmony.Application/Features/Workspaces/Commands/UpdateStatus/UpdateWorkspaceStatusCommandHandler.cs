using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Features.Workspaces.Commands.UpdateStatus;

namespace Harmony.Application.Features.Workspaces.Commands.Update
{
    /// <summary>
    /// Handler for creating workspace
    /// </summary>
    public class UpdateWorkspaceCommandHandler : IRequestHandler<UpdateWorkspaceStatusCommand, Result<bool>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<UpdateWorkspaceCommandHandler> _localizer;

        public UpdateWorkspaceCommandHandler(IWorkspaceRepository workspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<UpdateWorkspaceCommandHandler> localizer)
        {
            _workspaceRepository = workspaceRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }
        public async Task<Result<bool>> Handle(UpdateWorkspaceStatusCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if(string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var workspace = await _workspaceRepository.GetAsync(request.Id);

            if(workspace == null)
            {
                return await Result<bool>.FailAsync(_localizer["Workspace not found"]);
            }

            workspace.Status = request.Status;

            var dbResult = await _workspaceRepository.Update(workspace);

            if (dbResult > 0)
            {
                return await Result<bool>.SuccessAsync(true, _localizer["Workspace status updated"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
