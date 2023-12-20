using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Search.Queries.GlobalSearch
{
    /// <summary>
    /// Query for loading card
    /// </summary>
    public class GlobalSearchQuery : IRequest<IResult<GlobalSearchResponse>>
    {
        public GlobalSearchQuery(Guid boardId, string term)
        {
            BoardId = boardId;
            Term = term;
        }

        public Guid BoardId { get; set; }
        public string Term { get; set; }

        
    }
}