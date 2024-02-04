using Harmony.Application.DTO.Automation;
using Harmony.Application.Notifications;

namespace Harmony.Application.Contracts.Automation
{
    public interface ISmartAutoAssignAutomationService
    {
        Task Process(SmartAutoAssignAutomationDto automation, CardCreatedMessage notification);
    }
}
