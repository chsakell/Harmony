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
        Task<List<UserNotification>> GetAllForType(List<string> userIds, EmailNotificationType type);
        Task<List<string>> GetUsersForType(List<string> userIds, EmailNotificationType type);
        Task AddEntryAsync(UserNotification notification);
        void DeleteEntry(UserNotification notification);
        Task<List<UserNotification>> GetAllForUser(string userId);
        Task<UserNotification> GetForUser(string userId, EmailNotificationType type);
        Task<int> CreateAsync(UserNotification activity);
        Task<int> DeleteAsync(UserNotification activity);
        Task<int> Commit();
    }
}
