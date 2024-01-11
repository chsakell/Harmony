using Harmony.Application.DTO.Automation;
using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Search.Commands.AdvancedSearch;
using Harmony.Application.Features.Search.Queries.InitAdvancedSearch;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IAutomationManager : IManager
    {
        Task<IResult<List<AutomationTemplateDto>>> GetTemplates();
    }
}
