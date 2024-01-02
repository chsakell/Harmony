using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Search.Queries.InitAdvancedSearch
{
    /// <summary>
    /// Query for initializing advanced search
    /// </summary>
    public class InitAdvancedSearchQuery : IRequest<IResult<InitAdvancedSearchResponse>>
    {

    }
}