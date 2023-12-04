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

        public async Task<int> CreateAsync(UserNotification activity)
        {
            await _context.UserNotifications.AddAsync(activity);

            return await _context.SaveChangesAsync();
        }

        public async Task<List<UserNotification>> GetAllForUser(string userId)
        {
            return await _context.UserNotifications
                .Where(n => n.UserId == userId).ToListAsync();
        }

        public async Task<UserNotification?> GetForUser(string userId, NotificationType type)
        {
            return await _context.UserNotifications
                .Where(n => n.UserId == userId && n.NotificationType == type)
                .FirstOrDefaultAsync();
        }
    }
}
