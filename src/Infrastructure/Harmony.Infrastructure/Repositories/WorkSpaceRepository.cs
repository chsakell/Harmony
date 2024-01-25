using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly HarmonyContext _context;
        private readonly IUserService _userService;

        public WorkspaceRepository(HarmonyContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public IQueryable<Workspace> Entities => _context.Set<Workspace>();

        public async Task<Workspace?> GetAsync(Guid workspaceId)
        {
            return await _context.Workspaces
                .FirstOrDefaultAsync(w => w.Id == workspaceId);
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
            var user = await _userService.GetAsync(userId);

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

        public async Task<List<Board>> GetWorkspaceBoards(Guid workspaceId)
        {
            return await _context.Boards
                    .Include(board => board.Lists)
                        .ThenInclude(list => list.Cards)
                            .ThenInclude(card => card.CheckLists)
                                .ThenInclude(checkList => checkList.Items)
                    .Include(board => board.Users)
                .Where(Board => Board.WorkspaceId == workspaceId)
                .ToListAsync();
        }
    }
}
