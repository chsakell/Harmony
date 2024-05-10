using AutoMapper;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Responses;
using Harmony.Application.Specifications.Boards;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public GetBoardsHandler(IBoardRepository boardRepository,
            IBoardListRepository boardListRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<GetBoardsHandler> localizer,
            IBoardService boardService,
            ISprintRepository sprintRepository,
            IUserService userService,
            ICacheService cacheService,
            IMapper mapper)
        {
            _boardRepository = boardRepository;
            _boardListRepository = boardListRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _boardService = boardService;
            _sprintRepository = sprintRepository;
            _userService = userService;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<IResult<GetBoardResponse>> Handle(GetBoardQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            List<Sprint> activeSprints = null;
            Guid? selectedSprintId = null;

            if (string.IsNullOrEmpty(userId))
            {

                return await Result<GetBoardResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var board = await _cacheService.GetOrCreateAsync(CacheKeys.BoardSummary(request.BoardId),
            async () =>
            {
                var boardFilter = new BoardFilterSpecification()
                {
                    BoardId = request.BoardId,
                    IncludeWorkspace = true,
                    IncludeLists = true,
                    BoardListsStatuses = new List<BoardListStatus>() { BoardListStatus.Active },
                    IncludeLabels = true,
                    IncludeIssueTypes = true,
                };

                boardFilter.Build();

                var dbBoard = await _boardRepository
                    .Entities.AsNoTracking()
                    .Specify(boardFilter)
                    .FirstOrDefaultAsync();

                return dbBoard;
            }, TimeSpan.FromMinutes(1));

            if (board == null)
            {
                return await Result<GetBoardResponse>.FailAsync(_localizer["Board doesn't exist"]);
            }
            else if(board.Workspace.Status != Domain.Enums.WorkspaceStatus.Active)
            {
                return await Result<GetBoardResponse>.FailAsync($"The board's workspace '{board.Workspace.Name}' is inactive.", ResultCode.InactiveWorkspace);
            }

            bool userHasAccess = false;

            if(board.Type == BoardType.Retrospective)
            {
                await _boardRepository.LoadRetrospectiveEntryAsync(board);

                userHasAccess = await HasUserBoardAccess(board.Retrospective.ParentBoardId.Value, userId);
            }
            else
            {
                userHasAccess = await HasUserBoardAccess(request.BoardId, userId);
            }

            if (!userHasAccess)
            {
                return await Result<GetBoardResponse>.FailAsync(_localizer["You are not authorized to view this board's content."], ResultCode.UnauthorisedAccess);
            }

            if (board.Type == BoardType.Scrum)
            {
                activeSprints = await _sprintRepository.GetActiveSprints(request.BoardId);

                if(!activeSprints.Any())
                {
                    var emptyBoard = _mapper.Map<GetBoardResponse>(board);

                    emptyBoard.ActiveSprints = Enumerable.Empty<SprintDto>().ToList();

                    return await Result<GetBoardResponse>.SuccessAsync(emptyBoard);
                }
                else
                {
                    selectedSprintId = request.SprintId ?? activeSprints[0].Id;
                }
            }

            var userBoard = await _boardService
                .LoadBoard(board, request.MaxCardsPerList, selectedSprintId);

            var result = _mapper.Map<GetBoardResponse>(userBoard);
            
            Dictionary<Guid,int> totalCardsPerList = new Dictionary<Guid,int>();

            if (board.Type == BoardType.Scrum)
            {
                if (activeSprints != null && activeSprints.Any())
                {
                    result.ActiveSprints = _mapper.Map<List<SprintDto>>(activeSprints);
                    result.SelectedSprint = result.ActiveSprints.FirstOrDefault(s => s.Id == selectedSprintId);
                    
                    totalCardsPerList = await _boardListRepository
                            .GetTotalCardsForBoardLists(selectedSprintId.Value, result.Lists.Select(l => l.Id).ToList());
                }
            }
            else
            {
                totalCardsPerList = await _boardListRepository
                            .GetTotalCardsForBoardLists(result.Lists.Select(l => l.Id).ToList());
            }

            foreach(var list in result.Lists)
            {
                var totalCards = totalCardsPerList.ContainsKey(list.Id) ? totalCardsPerList[list.Id] : 0;

                list.TotalCards = totalCards;

                list.TotalPages = totalCardsPerList.ContainsKey(list.Id) ? (int)Math.Ceiling((double)totalCards / request.MaxCardsPerList) : 1;
            }

            var cards = result.Lists.SelectMany(l => l.Cards);

            var cardUserIds = cards.SelectMany(c => c.Members)
                    .Select(m => m.Id).Distinct();

            var cardUsers = await GetBoardUsers(board);

            foreach(var card in cards.Where(c => c.Members.Any()))
            {
                var users = cardUsers.Where(u => card.Members.Select(m => m.Id).Contains(u.Id)).Distinct();
                card.Members = _mapper.Map<List<CardMemberDto>>(users);
            }

            return await Result<GetBoardResponse>.SuccessAsync(result);
        }

        private async Task<List<UserResponse>> GetBoardUsers(Board board)
        {
            return await _cacheService.GetOrCreateAsync(CacheKeys.BoardMembers(board.Id), async () =>
            {
                var cards = board.Lists.SelectMany(l => l.Cards);

                var cardUserIds = cards.SelectMany(c => c.Members)
                        .Select(m => m.UserId).Distinct();

                var cardUsers = (await _userService.GetAllAsync(cardUserIds)).Data;

                return cardUsers;
            }, TimeSpan.FromMinutes(5));
        }

        private async Task<bool> HasUserBoardAccess(Guid boardId, string userId)
        {
            return await _cacheService.GetOrCreateAsync(CacheKeys.BoardMemberAccess(boardId, userId), async () =>
            {
                return await _boardService.HasUserAccessToBoard(userId, boardId);
            }, TimeSpan.FromMinutes(5));
        }
    }
}
