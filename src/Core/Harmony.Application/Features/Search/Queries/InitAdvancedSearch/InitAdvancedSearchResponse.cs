using Harmony.Application.DTO;

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