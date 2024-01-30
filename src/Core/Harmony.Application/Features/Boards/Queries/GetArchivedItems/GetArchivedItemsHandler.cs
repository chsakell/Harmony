using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Boards.Queries.GetArchivedItems
{
    public class GetArchivedItemsHandler : IRequestHandler<GetArchivedItemsQuery, PaginatedResult<GetArchivedItemResponse>>
    {
        private readonly ICardService _cardService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICardRepository _cardRepository;
        private readonly IStringLocalizer<GetArchivedItemsHandler> _localizer;

        public GetArchivedItemsHandler(ICardService cardService,
            ICurrentUserService currentUserService,
            ICardRepository cardRepository,
            IStringLocalizer<GetArchivedItemsHandler> localizer)
        {
            _cardService = cardService;
            _currentUserService = currentUserService;
            _cardRepository = cardRepository;
            _localizer = localizer;
        }

        public async Task<PaginatedResult<GetArchivedItemResponse>> Handle(GetArchivedItemsQuery request, CancellationToken cancellationToken)
        {
            request.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize == 0 ? 10 : request.PageSize;

            var totalArchivedItems = 
                await _cardRepository.CountBacklogCards(request.BoardId);

            var result = await _cardService.SearchArchivedItems(request.BoardId,
                 request.SearchTerm, request.PageNumber, request.PageSize);

            return PaginatedResult<GetArchivedItemResponse>
                .Success(result, totalArchivedItems, request.PageNumber, request.PageSize);
        }
    }
}
