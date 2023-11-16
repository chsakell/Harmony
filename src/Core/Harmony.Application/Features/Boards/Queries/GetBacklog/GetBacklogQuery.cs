using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.GetBacklog
{
    public class GetBacklogQuery : PagedRequest, IRequest<PaginatedResult<GetBacklogItemResponse>>
    {
        public Guid BoardId { get; set; }

        public GetBacklogQuery(Guid boardId)
        {
            BoardId = boardId;
        }

        public string SearchTerm { get; set; }

        public GetBacklogQuery(Guid boardId, int pageNumber, int pageSize, 
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