using Harmony.Application.DTO;
using Harmony.Application.DTO.Search;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Search.Queries.InitAdvancedSearch
{
    /// <summary>
    /// Response for initializaing advanced search
    /// </summary>
    public class InitAdvancedSearchResponse
    {
        public List<BoardDto> Boards {  get; set; }
    }
}