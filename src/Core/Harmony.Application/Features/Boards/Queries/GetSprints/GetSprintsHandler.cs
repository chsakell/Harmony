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
        private readonly IStringLocalizer<GetSprintsHandler> _localizer;

        public GetSprintsHandler(ICardService cardService,
            ICurrentUserService currentUserService,
            ICardRepository cardRepository,
            IStringLocalizer<GetSprintsHandler> localizer)
        {
            _cardService = cardService;
            _currentUserService = currentUserService;
            _cardRepository = cardRepository;
            _localizer = localizer;
        }

        public async Task<PaginatedResult<GetSprintItemResponse>> Handle(GetSprintsQuery request, CancellationToken cancellationToken)
        {
            request.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize == 0 ? 10 : request.PageSize;

            var totalWorkspaceUsers = 
                await _cardRepository.CountBacklogCards(request.BoardId);

            var result = await _cardService.SearchBacklog(request.BoardId,
                 request.SearchTerm, request.PageNumber, request.PageSize);

            return null;
            //return PaginatedResult<GetSprintItemResponse>
            //    .Success(result, totalWorkspaceUsers, request.PageNumber, request.PageSize);
        }
    }
}
