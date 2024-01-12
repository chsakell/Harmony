using Harmony.Application.DTO.Automation;
using Harmony.Domain.Automation;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IAutomationManager : IManager
    {
        Task<IResult<List<AutomationTemplateDto>>> GetTemplates();
        Task<IResult<List<T>>> GetAutomations<T>(Guid boardId, AutomationType type)
            where T : IAutomationDto;
    }
}
