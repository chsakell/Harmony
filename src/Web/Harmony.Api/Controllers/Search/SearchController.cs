using Harmony.Application.Features.Search.Commands.AdvancedSearch;
using Harmony.Application.Features.Search.Queries.GlobalSearch;
using Harmony.Application.Features.Search.Queries.InitAdvancedSearch;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers.Search
{
    /// <summary>
    /// Controller for searching
    /// </summary>
    public class SearchController : BaseApiController<SearchController>
    {
        [HttpGet]
        public async Task<IActionResult> Get(string text)
        {
            return Ok(await _mediator.Send(new GlobalSearchQuery(text)));
        }

        [HttpGet("advanced")]
        public async Task<IActionResult> InitAdvancedSearch()
        {
            return Ok(await _mediator.Send(new InitAdvancedSearchQuery()));
        }

        [HttpPost("advanced")]
        public async Task<IActionResult> AdvancedSearch(AdvancedSearchCommand command)
        {
            return Ok(await _mediator.Send(command));
        }
    }
}
