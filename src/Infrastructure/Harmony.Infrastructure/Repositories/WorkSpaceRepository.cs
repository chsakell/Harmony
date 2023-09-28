using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly HarmonyContext _context;

        public WorkspaceRepository(HarmonyContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Workspace workspace)
        {
            await _context.Workspaces.AddAsync(workspace);
        }

        public async Task<int> CreateAsync(Workspace workspace)
        {
            await _context.Workspaces.AddAsync(workspace);

            return await _context.SaveChangesAsync();
        }

        public async Task<List<Workspace>> GetAllForUser(string userId)
        {
            return await _context.Workspaces
                .Where(workspace => workspace.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Board>> LoadWorkspace(string userId, Guid workspaceId)
        {
            return await _context.Boards
                .Where(Board => Board.UserId == userId && Board.WorkspaceId == workspaceId)
                .ToListAsync();
        }
    }
}
