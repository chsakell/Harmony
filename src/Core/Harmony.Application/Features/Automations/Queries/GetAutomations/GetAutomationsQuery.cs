using Harmony.Application.DTO.Automation;
using Harmony.Domain.Enums.Automations;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Automations.Queries.GetAutomations
{
    /// <summary>
    /// Query for returning automations based on type
    /// </summary>
    public class GetAutomationsQuery : IRequest<IResult<IEnumerable<IAutomationDto>>>
    {
        public GetAutomationsQuery(AutomationType automationType, Guid boardId)
        {
            AutomationType = automationType;
            BoardId = boardId;
        }

        public AutomationType AutomationType { get; set; }
        public Guid BoardId { get; set; }
    }
}