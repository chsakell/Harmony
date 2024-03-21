using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Extensions;
using Harmony.Application.Features.Boards.Queries.GetSprintsSummary;
using Harmony.Application.Features.Sprints.Queries.GetSprintReports;
using Harmony.Application.Specifications.Sprints;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Boards.Queries.GetSprintsSummary
{
    public class GetSprintsSummaryHandler : IRequestHandler<GetSprintsSummaryQuery,
            PaginatedResult<SprintSummary>>
    {
        private readonly IBoardService _boardService;
        private readonly ISprintRepository _sprintRepository;
        private readonly IStringLocalizer<GetSprintReportsHandler> _localizer;

        public GetSprintsSummaryHandler(IBoardService boardService,
            ISprintRepository sprintRepository,
            IStringLocalizer<GetSprintReportsHandler> localizer)
        {
            _boardService = boardService;
            _sprintRepository = sprintRepository;
            _localizer = localizer;
        }

        public async Task<PaginatedResult<SprintSummary>> Handle(GetSprintsSummaryQuery request,
            CancellationToken cancellationToken)
        {
            request.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize == 0 ? 10 : request.PageSize;

            var filter = new SprintFilterSpecification(boardId: request.BoardId)
            {
                Status = request.Status
            };

            var totalSprints = await _sprintRepository
                .Entities.Specify(filter)
                .CountAsync();

            var result = await _boardService.GetSprintsSummaries(request.BoardId,
                 request.SearchTerm, request.PageNumber, request.PageSize, request.Status);

            return PaginatedResult<SprintSummary>
                .Success(result, totalSprints, request.PageNumber, request.PageSize);
        }
    }
}
