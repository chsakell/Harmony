using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Specifications.Cards;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Cards.Queries.SearchCards
{
    public class SearchCardsQueryHandler : IRequestHandler<SearchCardsQuery,
            PaginatedResult<CardDto>>
    {
        private readonly ICardRepository _cardRepository;
        private readonly IMapper _mapper;
        private readonly IBoardRepository _boardRepository;
        private readonly ICardService _cardService;
        private readonly IStringLocalizer<SearchCardsQueryHandler> _localizer;

        public SearchCardsQueryHandler(ICardRepository cardRepository,
            IMapper mapper,
            IBoardRepository boardRepository,
            ICardService cardService,
            ISprintService sprintService,
            IStringLocalizer<SearchCardsQueryHandler> localizer)
        {
            _cardRepository = cardRepository;
            _mapper = mapper;
            _boardRepository = boardRepository;
            _cardService = cardService;
            _localizer = localizer;
        }

        public async Task<PaginatedResult<CardDto>> Handle(SearchCardsQuery request,
            CancellationToken cancellationToken)
        {
            request.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize == 0 ? 10 : request.PageSize;

            var board = await _boardRepository.GetAsync(request.BoardId);

            var totalCards = await _cardService
                .CountWorkspaceCards(board.WorkspaceId, request.SearchTerm, request.SkipCardId);

            var result = await _cardService.SearchWorkspaceCards(board.WorkspaceId,
                 request.SearchTerm, request.PageNumber, request.PageSize, request.SkipCardId);

            return PaginatedResult<CardDto>
                .Success(result, totalCards, request.PageNumber, request.PageSize);
        }
    }
}
