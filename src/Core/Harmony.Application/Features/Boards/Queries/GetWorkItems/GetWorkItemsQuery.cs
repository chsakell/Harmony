using Harmony.Application.DTO;
using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.GetWorkItems
{
    public class GetWorkItemsQuery : PagedRequest, IRequest<PaginatedResult<CardDto>>
    {
        public Guid BoardId { get; set; }
        public string CardTitle { get; set; }
        public List<Guid>? IssueTypes { get; set; }
        public List<Guid>? BoardLists { get; set; }

        public GetWorkItemsQuery()
        {
            
        }

        public GetWorkItemsQuery(Guid boardId)
        {
            BoardId = boardId;
        }

        public GetWorkItemsQuery(Guid boardId, int pageNumber, int pageSize, string orderBy)
        {
            BoardId = boardId;
            PageNumber = pageNumber;
            PageSize = pageSize;

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                OrderBy = orderBy.Split(',');
            }
        }
    }
}