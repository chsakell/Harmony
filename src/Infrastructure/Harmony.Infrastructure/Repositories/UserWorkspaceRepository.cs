using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class UserWorkspaceRepository : IUserWorkspaceRepository
    {
        private readonly HarmonyContext _context;

        public UserWorkspaceRepository(HarmonyContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetWorkspaceUsers(Guid workspaceId, int pageNumber, int pageSize)
        {
            return await _context.UserWorkspaces
                .Where(userWorkspace => userWorkspace.WorkspaceId == workspaceId)
                .Select(userWorkspace => userWorkspace.UserId)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountWorkspaceUsers(Guid workspaceId)
        {
            return await _context.UserWorkspaces
                .Where(userWorkspace => userWorkspace.WorkspaceId == workspaceId)
                .CountAsync();
        }

        public async Task<int> CreateAsync(UserWorkspace userWorkspaceRepository)
        {
            await _context.UserWorkspaces.AddAsync(userWorkspaceRepository);

            return await _context.SaveChangesAsync();
        }

        public async Task<List<string>> SearchWorkspaceUsers(Guid workspaceId, List<string> userIds)
        {
            return await _context.UserWorkspaces
                .Where(userWorkspace => userWorkspace.WorkspaceId == workspaceId
                    && userIds.Contains(userWorkspace.UserId))
                .Select(userworkspace => userworkspace.UserId)
                .ToListAsync();
        }

        public async Task<UserWorkspace?> GetUserWorkspace(Guid workspaceId, string userId)
        {
            return await _context.UserWorkspaces
                .Where(userWorkspace => userWorkspace.WorkspaceId == workspaceId
                    && userWorkspace.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> RemoveAsync(UserWorkspace userWorkspaceRepository)
        {
            _context.UserWorkspaces.Remove(userWorkspaceRepository);

            return await _context.SaveChangesAsync();
        }
    }
}
