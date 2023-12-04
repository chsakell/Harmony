using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access User notifications
    /// </summary>
    public interface IUserNotificationRepository
    {
        IQueryable<UserNotification> Entities { get; }
        Task<List<UserNotification>> GetAllForUser(string userId);
        Task<UserNotification> GetForUser(string userId, NotificationType type);
        Task<int> CreateAsync(UserNotification activity);
    }
}
