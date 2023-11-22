using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Workspaces.Queries.GetAllForUser
{
    public class GetUserWorkspacesHandler : IRequestHandler<GetUserWorkspacesQuery, IResult<List<WorkspaceDto>>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBoardService _boardService;
        private readonly IStringLocalizer<GetUserWorkspacesHandler> _localizer;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetUserWorkspacesHandler(IWorkspaceRepository workspaceRepository,
            ICurrentUserService currentUserService,
            IBoardService boardService,
            IStringLocalizer<GetUserWorkspacesHandler> localizer,
            IUserService userService,
            IMapper mapper)
        {
            _workspaceRepository = workspaceRepository;
            _currentUserService = currentUserService;
            _boardService = boardService;
            _localizer = localizer;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<IResult<List<WorkspaceDto>>> Handle(GetUserWorkspacesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<WorkspaceDto>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userAccessWorkspaces = await _userService.GetAccessWorkspacesAsync(userId);

            if(!userAccessWorkspaces.Succeeded)
            {
                return await Result<List<WorkspaceDto>>.FailAsync(userAccessWorkspaces.Messages);
            }

            foreach (var workspace in userAccessWorkspaces.Data)
            {
                var workspaceBoards = await _boardService.GetUserBoards(workspace.Id, userId);

                if(workspaceBoards.Any())
                {
                    workspace.Boards = workspaceBoards;
                }
            }

            var result = _mapper.Map<List<WorkspaceDto>>(userAccessWorkspaces.Data);

            return await Result<List<WorkspaceDto>>.SuccessAsync(result);
        }
    }
}
