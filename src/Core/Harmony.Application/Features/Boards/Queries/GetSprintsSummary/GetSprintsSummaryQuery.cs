using Harmony.Application.Requests;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.GetSprintsSummary
{
    public class GetSprintsSummaryQuery : PagedRequest, IRequest<PaginatedResult<SprintSummary>>
    {
        public GetSprintsSummaryQuery(Guid boardId, int pageNumber, int pageSize,
            string searchTerm, string orderBy)
        {
            BoardId = boardId;
            PageNumber = pageNumber;
            PageSize = pageSize;
            SearchTerm = searchTerm;

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                OrderBy = orderBy.Split(',');
            }
        }

        public GetSprintsSummaryQuery(Guid boardId)
        {

        }

        public string SearchTerm { get; set; }
        public Guid BoardId { get; set; }
        public SprintStatus? Status { get; set; }
    }
}