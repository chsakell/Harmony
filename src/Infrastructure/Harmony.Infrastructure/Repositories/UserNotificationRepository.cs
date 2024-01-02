using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class UserNotificationRepository : IUserNotificationRepository
    {
        private readonly HarmonyContext _context;

        public UserNotificationRepository(HarmonyContext context)
        {
            _context = context;
        }

        public IQueryable<UserNotification> Entities => _context.Set<UserNotification>();

        public async Task AddEntryAsync(UserNotification notification)
        {
            await _context.UserNotifications.AddAsync(notification);
        }

        public async Task<int> CreateAsync(UserNotification activity)
        {
            await _context.UserNotifications.AddAsync(activity);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(UserNotification activity)
        {
            _context.Remove(activity);

            return await _context.SaveChangesAsync();
        }

        public void DeleteEntry(UserNotification notification)
        {
            _context.UserNotifications.Remove(notification);
        }

        public async Task<List<UserNotification>> GetAllForUser(string userId)
        {
            return await _context.UserNotifications
                .Where(n => n.UserId == userId).ToListAsync();
        }

        public async Task<List<UserNotification>> GetAllForType(List<string> userIds, EmailNotificationType type)
        {
            return await _context.UserNotifications
                .Where(n => userIds.Contains(n.UserId) && n.NotificationType == type)
                .ToListAsync();
        }

        public async Task<List<string>> GetUsersForType(List<string> userIds, EmailNotificationType type)
        {
            return await _context.UserNotifications
                .Where(n => userIds.Contains(n.UserId) && n.NotificationType == type)
                .Select(n => n.UserId)
                .ToListAsync();
        }

        public async Task<UserNotification?> GetForUser(string userId, EmailNotificationType type)
        {
            return await _context.UserNotifications
                .Where(n => n.UserId == userId && n.NotificationType == type)
                .FirstOrDefaultAsync();
        }

        public async Task<int> Commit()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
