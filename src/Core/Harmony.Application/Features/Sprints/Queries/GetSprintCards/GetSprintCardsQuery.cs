using Harmony.Application.DTO;
using Harmony.Application.Requests;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Sprints.Queries.GetSprintCards
{
    public class GetSprintCardsQuery : PagedRequest, IRequest<PaginatedResult<CardDto>>
    {
        public GetSprintCardsQuery(Guid sprintId, int pageNumber, int pageSize,
            string searchTerm, string orderBy)
        {
            SprintId = sprintId;
            PageNumber = pageNumber;
            PageSize = pageSize;
            SearchTerm = searchTerm;

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                OrderBy = orderBy.Split(',');
            }
        }

        public GetSprintCardsQuery(Guid sprintId)
        {
            SprintId = sprintId;
        }

        public string SearchTerm { get; set; }
        public Guid SprintId { get; set; }
        public CardStatus? Status { get; set; }
    }
}