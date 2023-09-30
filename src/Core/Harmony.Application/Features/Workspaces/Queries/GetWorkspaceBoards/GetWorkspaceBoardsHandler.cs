using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards
{
    public class GetWorkspaceBoardsHandler : IRequestHandler<GetWorkspaceBoardsQuery, IResult<List<GetWorkspaceBoardResponse>>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<GetWorkspaceBoardsHandler> _localizer;
        private readonly IMapper _mapper;

        public GetWorkspaceBoardsHandler(IWorkspaceRepository workspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<GetWorkspaceBoardsHandler> localizer,
            IMapper mapper)
        {
            _workspaceRepository = workspaceRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<List<GetWorkspaceBoardResponse>>> Handle(GetWorkspaceBoardsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<GetWorkspaceBoardResponse>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var workspaceBoards = await _workspaceRepository.GetWorkspaceBoards(request.WorkspaceId);

            var result = _mapper.Map<List<GetWorkspaceBoardResponse>>(workspaceBoards);

            return await Result<List<GetWorkspaceBoardResponse>>.SuccessAsync(result);
        }
    }
}
