using AutoMapper;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetUserBoards;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Boards.Queries.SearchBoardUsers
{
    /// <summary>
    /// Handler for searching a board's users
    /// </summary>
    public class GetUserBoardsHandler : IRequestHandler<GetUserBoardsQuery, Result<List<BoardDto>>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IBoardService _boardService;
        private readonly IStringLocalizer<GetUserBoardsHandler> _localizer;
        private readonly IMapper _mapper;

        public GetUserBoardsHandler(ICurrentUserService currentUserService,
            IBoardService boardService,
            IStringLocalizer<GetUserBoardsHandler> localizer,
            IMapper mapper)
        {
            _currentUserService = currentUserService;
            _boardService = boardService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<Result<List<BoardDto>>> Handle(GetUserBoardsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<BoardDto>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoards = await _boardService.GetUserBoardsWithLists(null, userId);

            var result = _mapper.Map<List<BoardDto>>(userBoards);

            return await Result<List<BoardDto>>.SuccessAsync(result);
        }
    }
}
