using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Workspaces.Queries.LoadWorkspace
{
    public class LoadWorkspaceHandler : IRequestHandler<LoadWorkspaceQuery, IResult<List<BoardDto>>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBoardService _boardService;
        private readonly ICardActivityService _cardActivityService;
        private readonly IStringLocalizer<LoadWorkspaceHandler> _localizer;
        private readonly IMapper _mapper;

        public LoadWorkspaceHandler(IWorkspaceRepository workspaceRepository,
            IUserWorkspaceRepository userWorkspaceRepository,
            ICurrentUserService currentUserService,
            IBoardService boardService,
            ICardActivityService cardActivityService,
            IStringLocalizer<LoadWorkspaceHandler> localizer,
            IMapper mapper)
        {
            _workspaceRepository = workspaceRepository;
            _userWorkspaceRepository = userWorkspaceRepository;
            _currentUserService = currentUserService;
            _boardService = boardService;
            _cardActivityService = cardActivityService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<List<BoardDto> >> Handle(LoadWorkspaceQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<BoardDto> >.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoards = await _boardService.GetUserBoards(request.WorkspaceId, userId);

            var boardInfos = await _boardService
                .GetStatusForBoards(userBoards.Select(b => b.Id).ToList());

            var result = _mapper.Map<List<BoardDto>>(boardInfos);

            return await Result<List<BoardDto> >.SuccessAsync(result);
        }
    }
}
