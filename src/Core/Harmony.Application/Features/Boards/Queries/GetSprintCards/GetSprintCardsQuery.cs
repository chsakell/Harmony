using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Requests;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.GetSprints
{
    public class GetSprintCardsQuery : PagedRequest, IRequest<PaginatedResult<GetSprintCardResponse>>
    {
        public Guid BoardId { get; set; }
        public SprintStatus? SprintStatus { get; set; }

        public GetSprintCardsQuery(Guid boardId)
        {
            BoardId = boardId;
        }

        public string SearchTerm { get; set; }

        public GetSprintCardsQuery(Guid boardId, int pageNumber, int pageSize, 
            string searchTerm, string orderBy, SprintStatus? status)
        {
            BoardId = boardId;
            PageNumber = pageNumber;
            PageSize = pageSize;
            SearchTerm = searchTerm;
            SprintStatus = status;

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                OrderBy = orderBy.Split(',');
            }
        }
    }
}