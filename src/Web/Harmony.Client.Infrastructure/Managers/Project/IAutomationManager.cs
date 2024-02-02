using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Application.Features.Automations.Commands.ToggleAutomation;
using Harmony.Domain.Enums.Automations;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IAutomationManager : IManager
    {
        Task<IResult<List<AutomationTemplateDto>>> GetTemplates();
        Task<IResult<List<T>>> GetAutomations<T>(Guid boardId, AutomationType type)
            where T : IAutomationDto;

        Task<IResult<string>> CreateAutomation(CreateAutomationCommand command);
        Task<IResult<bool>> RemoveAutomation(string automationId);
        Task<IResult<bool>> ToggleAutomation(ToggleAutomationCommand command);
    }
}
