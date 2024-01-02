using Harmony.Application.DTO.Search;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Search.Commands.AdvancedSearch
{
    public class AdvancedSearchCommand : IRequest<IResult<List<SearchableCard>>>
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Comment { get; set; }

        public List<string> Members { get; set; }

        public Guid? BoardId { get; set; }

        public Guid? ListId { get; set; }

        public bool HasAttachments { get; set; }

        public CardStatus CardStatus { get; set; }

        public DateTime? DueDate { get; set; }
        public bool CombineFilters { get; set; }
    }
}
