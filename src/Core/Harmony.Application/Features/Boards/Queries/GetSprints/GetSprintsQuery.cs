using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.GetSprints
{
    public class GetSprintsQuery : PagedRequest, IRequest<PaginatedResult<GetSprintItemResponse>>
    {
        public Guid BoardId { get; set; }

        public GetSprintsQuery(Guid boardId)
        {
            BoardId = boardId;
        }

        public string SearchTerm { get; set; }

        public GetSprintsQuery(Guid boardId, int pageNumber, int pageSize, 
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
    }
}