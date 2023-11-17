using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Application.Responses;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Boards.Queries.GetSprints
{
    public class GetSprintsHandler : IRequestHandler<GetSprintsQuery, PaginatedResult<GetSprintItemResponse>>
    {
        private readonly ICardService _cardService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICardRepository _cardRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly IBoardService _boardService;
        private readonly IStringLocalizer<GetSprintsHandler> _localizer;

        public GetSprintsHandler(ICardService cardService,
            ICurrentUserService currentUserService,
            ICardRepository cardRepository,
            ISprintRepository sprintRepository,
            IBoardService boardService,
            IStringLocalizer<GetSprintsHandler> localizer)
        {
            _cardService = cardService;
            _currentUserService = currentUserService;
            _cardRepository = cardRepository;
            _sprintRepository = sprintRepository;
            _boardService = boardService;
            _localizer = localizer;
        }

        public async Task<PaginatedResult<GetSprintItemResponse>> Handle(GetSprintsQuery request, CancellationToken cancellationToken)
        {
            request.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize == 0 ? 10 : request.PageSize;

            var totalSprints = 
                await _sprintRepository.CountSprints(request.BoardId);

            var result = await _boardService.SearchSprints(request.BoardId,
                 request.SearchTerm, request.PageNumber, request.PageSize);

            return PaginatedResult<GetSprintItemResponse>
                .Success(result, totalSprints, request.PageNumber, request.PageSize);
        }
    }
}
