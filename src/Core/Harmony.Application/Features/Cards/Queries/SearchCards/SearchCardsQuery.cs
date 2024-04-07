using Harmony.Application.DTO;
using Harmony.Application.Requests;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Queries.SearchCards
{
    public class SearchCardsQuery : PagedRequest, IRequest<PaginatedResult<CardDto>>
    {
        public SearchCardsQuery(int pageNumber, int pageSize,
            string searchTerm, string orderBy)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            SearchTerm = searchTerm;

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                OrderBy = orderBy.Split(',');
            }
        }

        public SearchCardsQuery(Guid boardId)
        {
            BoardId = boardId;
        }

        public Guid? SkipCardId { get; set; }
        public Guid BoardId { get; set; }
        public string SearchTerm { get; set; }
        public CardStatus? Status { get; set; }
    }
}