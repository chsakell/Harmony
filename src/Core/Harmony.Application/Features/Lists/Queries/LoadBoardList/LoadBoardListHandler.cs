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
using Harmony.Domain.Extensions;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Lists.Queries.LoadBoardList
{
    /// <summary>
    /// Handler for returning board list cards
    /// </summary>
    public class GetBoardListHandler : IRequestHandler<LoadBoardListQuery, IResult<List<CardDto>>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IBoardListRepository _boardListRepository;
        private readonly IBoardService _boardService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICacheService _cacheService;
        private readonly IStringLocalizer<GetBoardListHandler> _localizer;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetBoardListHandler(IBoardRepository boardRepository,
            IBoardListRepository boardListRepository,
            IBoardService boardService,
            ICurrentUserService currentUserService,
            ICacheService cacheService,
            IStringLocalizer<GetBoardListHandler> localizer,
            IUserService userService,
            IMapper mapper)
        {
            _boardRepository = boardRepository;
            _boardListRepository = boardListRepository;
            _boardService = boardService;
            _currentUserService = currentUserService;
            _cacheService = cacheService;
            _localizer = localizer;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<IResult<List<CardDto>>> Handle(LoadBoardListQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {

                return await Result<List<CardDto>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var board = await _cacheService.HashGetAllOrCreateAsync(CacheKeys.Board(request.BoardId),
                dict => CacheDomainExtensions.FromDictionary(request.BoardId, dict),
                async () => await _boardService.GetBoard(request.BoardId));

            if (board == null)
            {
                return await Result<List<CardDto>>.FailAsync(_localizer["Board doesn't exist"]);
            }

            var cards = await _boardService
                    .LoadBoardListCards(board, request.BoardListId, 
                    request.Page, request.PageSize, request.SprintId);

            var result = _mapper.Map<List<CardDto>>(cards);

            var cardUserIds = result.SelectMany(c => c.Members)
                    .Select(m => m.Id).Distinct();

            var cardUsers = await GetBoardUsers(board);

            foreach (var card in result.Where(c => c.Members.Any()))
            {
                var users = cardUsers.Where(u => card.Members.Select(m => m.Id).Contains(u.Id)).Distinct();
                card.Members = _mapper.Map<List<CardMemberDto>>(users);
            }

            return await Result<List<CardDto>>.SuccessAsync(result);
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
