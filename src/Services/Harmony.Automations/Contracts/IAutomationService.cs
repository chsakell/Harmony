using Harmony.Application.Notifications;

namespace Harmony.Automations.Contracts
{
    public interface IAutomationService<T>
    {
        Task Run(T notification);
    }
}
