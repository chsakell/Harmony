using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Boards.Queries.GetSprints
{
    public class GetSprintCardsHandler : IRequestHandler<GetSprintCardsQuery, PaginatedResult<GetSprintCardResponse>>
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly IBoardService _boardService;
        private readonly IStringLocalizer<GetSprintCardsHandler> _localizer;

        public GetSprintCardsHandler(ISprintRepository sprintRepository,
            IBoardService boardService,
            IStringLocalizer<GetSprintCardsHandler> localizer)
        {
            _sprintRepository = sprintRepository;
            _boardService = boardService;
            _localizer = localizer;
        }

        public async Task<PaginatedResult<GetSprintCardResponse>> Handle(GetSprintCardsQuery request, CancellationToken cancellationToken)
        {
            request.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize == 0 ? 10 : request.PageSize;

            var totalSprints = 
                await _sprintRepository.CountSprints(request.BoardId);

            var result = await _boardService.SearchSprints(request.BoardId,
                 request.SearchTerm, request.PageNumber, request.PageSize, request.SprintStatus);

            return PaginatedResult<GetSprintCardResponse>
                .Success(result, totalSprints, request.PageNumber, request.PageSize);
        }
    }
}
