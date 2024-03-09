using Harmony.Application.DTO.Automation;
using Harmony.Application.Notifications;

namespace Harmony.Application.Contracts.Automation
{
    public interface ISumUpStoryPointsAutomationService
    {
        Task Process(SumUpStoryPointsAutomationDto automation, CardStoryPointsChangedMessage notification);
    }
}
