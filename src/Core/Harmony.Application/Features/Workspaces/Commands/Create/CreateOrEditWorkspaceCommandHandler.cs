using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;

namespace Harmony.Application.Features.Workspaces.Commands.Create
{
    /// <summary>
    /// Handler for creating workspace
    /// </summary>
    public class CreateOrEditWorkspaceCommandHandler : IRequestHandler<CreateOrEditWorkspaceCommand, Result<WorkspaceDto>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IMapper _mapper;
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<CreateOrEditWorkspaceCommandHandler> _localizer;

        public CreateOrEditWorkspaceCommandHandler(IWorkspaceRepository workspaceRepository,
            IMapper mapper,
            IUserWorkspaceRepository userWorkspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<CreateOrEditWorkspaceCommandHandler> localizer)
        {
            _workspaceRepository = workspaceRepository;
            _mapper = mapper;
            _userWorkspaceRepository = userWorkspaceRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }
        public async Task<Result<WorkspaceDto>> Handle(CreateOrEditWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if(string.IsNullOrEmpty(userId))
            {
                return await Result<WorkspaceDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            if(request.WorkspaceId.HasValue)
            {
                var workspaceDb = await _workspaceRepository.GetAsync(request.WorkspaceId.Value);

                if(workspaceDb != null)
                {
                    workspaceDb.Name = request.Name.Trim();
                    workspaceDb.Description = request.Description.Trim();
                    workspaceDb.IsPublic = request.IsPublic;

                    var updateResult = await _workspaceRepository.Update(workspaceDb);

                    if (updateResult > 0)
                    {
                        var result = _mapper.Map<WorkspaceDto>(workspaceDb);

                        return await Result<WorkspaceDto>.SuccessAsync(result, _localizer["Workspace updated"]);
                    }

                    return await Result<WorkspaceDto>.FailAsync(_localizer["Operation failed"]);
                }
            }

            var workspace = new Workspace()
            {
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                UserId = userId,
                IsPublic = request.IsPublic,
            };

            await _workspaceRepository.AddAsync(workspace);

            var userWorkspace = new UserWorkspace()
            {
                UserId = userId,
                WorkspaceId = workspace.Id
            };

            var dbResult = await _userWorkspaceRepository.CreateAsync(userWorkspace);

            if (dbResult > 0)
            {
                var result = _mapper.Map<WorkspaceDto>(workspace);

                return await Result<WorkspaceDto>.SuccessAsync(result, _localizer["Workspace created"]);
            }

            return await Result<WorkspaceDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
