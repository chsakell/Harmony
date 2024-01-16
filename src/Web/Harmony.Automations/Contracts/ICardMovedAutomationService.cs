using Harmony.Application.Notifications;

namespace Harmony.Automations.Contracts
{
    public interface ICardMovedAutomationService
    {
        Task Run(CardMovedNotification notification);
    }
}
