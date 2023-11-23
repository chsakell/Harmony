using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Boards.Queries.Get
{
    /// <summary>
    /// Handler for getting a board
    /// </summary>
    public class GetBoardsHandler : IRequestHandler<GetBoardQuery, IResult<GetBoardResponse>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<GetBoardsHandler> _localizer;
        private readonly IBoardService _boardService;
        private readonly ISprintRepository _sprintRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetBoardsHandler(IBoardRepository boardRepository,
            IBoardListRepository boardListRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<GetBoardsHandler> localizer,
            IBoardService boardService,
            ISprintRepository sprintRepository,
            IUserService userService,
            IMapper mapper)
        {
            _boardRepository = boardRepository;
            _boardListRepository = boardListRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _boardService = boardService;
            _sprintRepository = sprintRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<IResult<GetBoardResponse>> Handle(GetBoardQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            List<Sprint> activeSprints = null;

            if (string.IsNullOrEmpty(userId))
            {

                return await Result<GetBoardResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var board = await _boardRepository.GetAsync(request.BoardId);

            if (board == null)
            {
                return await Result<GetBoardResponse>.FailAsync(_localizer["Board doesn't exist"]);
            }

            var userHasAccess = await _boardService.HasUserAccessToBoard(userId, request.BoardId);

            if (!userHasAccess)
            {
                return await Result<GetBoardResponse>.FailAsync(_localizer["You are not authorized to view this board's content."], ResultCode.UnauthorisedAccess);
            }

            if(board.Type == Domain.Enums.BoardType.Scrum)
            {
                activeSprints = await _sprintRepository.GetActiveSprints(request.BoardId);

                if(!activeSprints.Any())
                {
                    board = await _boardRepository.GetBoardWithLists(request.BoardId);
                    var emptyBoard = _mapper.Map<GetBoardResponse>(board);

                    emptyBoard.ActiveSprints = Enumerable.Empty<SprintDto>().ToList();

                    return await Result<GetBoardResponse>.SuccessAsync(emptyBoard);
                }
            }

            var userBoard = await _boardService.LoadBoard(request.BoardId, request.MaxCardsPerList);

            var result = _mapper.Map<GetBoardResponse>(userBoard);
            
            if(activeSprints != null && activeSprints.Any())
            {
                result.ActiveSprints = _mapper.Map<List<SprintDto>>(activeSprints);
            }

            var totalCardsPerList = await _boardListRepository
                        .GetTotalCardsForBoardLists(result.Lists.Select(l => l.Id).ToList());

            foreach(var list in result.Lists)
            {
                var totalCards = totalCardsPerList[list.Id];

                list.TotalCards = totalCardsPerList[list.Id];

                list.TotalPages = (int)Math.Ceiling((double)totalCards / request.MaxCardsPerList);
            }

            var cards = result.Lists.SelectMany(l => l.Cards);

            var cardUserIds = cards.SelectMany(c => c.Members)
                    .Select(m => m.Id).Distinct();

            var cardUsers = (await _userService.GetAllAsync(cardUserIds)).Data;

            foreach(var card in cards.Where(c => c.Members.Any()))
            {
                var users = cardUsers.Where(u => card.Members.Select(m => m.Id).Contains(u.Id)).Distinct();
                card.Members = _mapper.Map<List<CardMemberDto>>(users);
            }

            return await Result<GetBoardResponse>.SuccessAsync(result);
        }
    }
}
