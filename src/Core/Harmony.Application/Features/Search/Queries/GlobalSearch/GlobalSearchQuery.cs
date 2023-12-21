using Harmony.Application.DTO.Search;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Search.Queries.GlobalSearch
{
    /// <summary>
    /// Query for searching cards
    /// </summary>
    public class GlobalSearchQuery : IRequest<IResult<List<SearchableCard>>>
    {
        public GlobalSearchQuery(string term)
        {
            Term = term;
        }

        public string Term { get; set; }    
    }
}