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
    public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, Result<WorkspaceDto>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IMapper _mapper;
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<CreateWorkspaceCommandHandler> _localizer;

        public CreateWorkspaceCommandHandler(IWorkspaceRepository workspaceRepository,
            IMapper mapper,
            IUserWorkspaceRepository userWorkspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<CreateWorkspaceCommandHandler> localizer)
        {
            _workspaceRepository = workspaceRepository;
            _mapper = mapper;
            _userWorkspaceRepository = userWorkspaceRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }
        public async Task<Result<WorkspaceDto>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if(string.IsNullOrEmpty(userId))
            {
                return await Result<WorkspaceDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var workspace = new Workspace()
            {
                Name = request.Name,
                Description = request.Description,
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
