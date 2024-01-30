using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.GetArchivedItems
{
    public class GetArchivedItemsQuery : PagedRequest, IRequest<PaginatedResult<GetArchivedItemResponse>>
    {
        public Guid BoardId { get; set; }

        public GetArchivedItemsQuery(Guid boardId)
        {
            BoardId = boardId;
        }

        public string SearchTerm { get; set; }

        public GetArchivedItemsQuery(Guid boardId, int pageNumber, int pageSize,
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