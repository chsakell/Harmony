using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IUserNotificationManager : IManager
    {
        /// <summary>The logged-in user's own notification settings.</summary>
        Task<IResult<List<EmailNotificationType>>> GetNotificationsAsync();
        Task<IResult<bool>> SetNotificationsAsync(List<EmailNotificationType> notifications);
    }
}
