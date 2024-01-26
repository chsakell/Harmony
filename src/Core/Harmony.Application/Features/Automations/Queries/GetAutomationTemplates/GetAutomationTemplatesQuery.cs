using Harmony.Application.DTO.Automation;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Automations.Queries.GetAutomationTemplates
{
    /// <summary>
    /// Query for returning automation templates
    /// </summary>
    public class GetAutomationTemplatesQuery : IRequest<IResult<List<AutomationTemplateDto>>>
    {
        public GetAutomationTemplatesQuery()
        {

        }
    }
}