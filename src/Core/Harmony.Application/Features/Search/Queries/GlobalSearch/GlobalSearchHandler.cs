using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.DTO;
using Harmony.Application.DTO.Search;
using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Search.Queries.GlobalSearch
{
    /// <summary>
    /// Handler for loading card
    /// </summary>
    public class GlobalSearchHandler : IRequestHandler<GlobalSearchQuery, IResult<List<SearchableCard>>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        private readonly ISearchService _searchService;
        private readonly IBoardService _boardService;
        private readonly IStringLocalizer<GlobalSearchHandler> _localizer;
        private readonly IMapper _mapper;

        public GlobalSearchHandler(ICurrentUserService currentUserService,
            IUserService userService,
            ISearchService searchService,
            IBoardService boardService,
            IStringLocalizer<GlobalSearchHandler> localizer,
            IMapper mapper)
        {
            _currentUserService = currentUserService;
            _userService = userService;
            _searchService = searchService;
            _boardService = boardService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<List<SearchableCard>>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var result = new List<SearchableCard>();

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<SearchableCard>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoards = await _boardService.GetUserBoards(null, userId);
            var boardIds = userBoards.Select(b => b.Id).ToList();

            var indexedCards = await _searchService.Search(boardIds, request.Term);
            List<BoardInfo> boardInfos = new List<BoardInfo>();
            foreach(var boardId in boardIds) 
            {
                var boardInfo = await _boardService.GetBoardInfo(boardId);
                if(boardInfo != null)
                {
                    boardInfos.Add(boardInfo);
                }
            }

            foreach(var indexedCard in  indexedCards)
            {
                var searchableCard = new SearchableCard()
                {
                    CardId = indexedCard.ObjectID,
                    Title = indexedCard.Title,
                    IssueType = indexedCard.IssueType,
                    Status = indexedCard.Status,
                    SerialKey = indexedCard.SerialKey
                };

                var board = boardInfos.FirstOrDefault(bi => bi.Id == Guid.Parse(indexedCard.BoardId));

                if(board != null)
                {
                    searchableCard.BoardTitle = board.Title;
                    searchableCard.BoardId = board.Id;

                    var list = board.Lists.FirstOrDefault(l => l.Id == indexedCard.ListId);

                    if(list != null)
                    {
                        searchableCard.List = list.Title;

                        searchableCard.IsComplete = list.CardStatus == Domain.Enums.BoardListCardStatus.DONE;
                    }
                }

                result.Add(searchableCard);
            }

            return await Result<List<SearchableCard>>.SuccessAsync(result);
        }
    }
}
