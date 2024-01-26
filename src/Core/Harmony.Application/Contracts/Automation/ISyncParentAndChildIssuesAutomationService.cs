using Harmony.Application.DTO.Automation;
using Harmony.Application.Notifications;

namespace Harmony.Application.Contracts.Automation
{
    public interface ISyncParentAndChildIssuesAutomationService
    {
        Task Process(SyncParentAndChildIssuesAutomationDto automation, CardMovedMessage notification);
    }
}
