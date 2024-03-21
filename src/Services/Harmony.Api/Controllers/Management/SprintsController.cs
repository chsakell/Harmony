using Harmony.Application.Features.Boards.Queries.GetSprintsSummary;
using Harmony.Application.Features.Sprints.Commands.CompleteSprint;
using Harmony.Application.Features.Sprints.Commands.StartSprint;
using Harmony.Application.Features.Sprints.Queries.GetSprintCards;
using Harmony.Application.Features.Sprints.Queries.GetSprintReports;
using Harmony.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers.Management
{
    /// <summary>
    /// Controller for Sprint operations
    /// </summary>
    public class SprintsController : BaseApiController<SprintsController>
    {
        private readonly ISender _sender;

        public SprintsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("{id:guid}/reports")]
        public async Task<IActionResult> GetReports(Guid id)
        {
            return Ok(await _sender.Send(new GetSprintReportsQuery(id)));
        }

        [HttpGet("{id:guid}/cards")]
        public async Task<IActionResult> GetCards(Guid id, int pageNumber, int pageSize,
            string searchTerm = null, string orderBy = null,
            CardStatus? status = null)
        {
            var sprintCardsQuery = new
                GetSprintCardsQuery(id, pageNumber, pageSize, searchTerm, orderBy)
            {
                Status = status
            };

            return Ok(await _sender.Send(sprintCardsQuery));
        }

        [HttpPut("{id:guid}/start")]
        public async Task<IActionResult> Put(Guid id, StartSprintCommand command)
        {
            return Ok(await _sender.Send(command));
        }

        [HttpPost("{id:guid}/complete")]
        public async Task<IActionResult> CompleteSprint(Guid id, CompleteSprintCommand command)
        {
            return Ok(await _sender.Send(command));
        }
    }
}
