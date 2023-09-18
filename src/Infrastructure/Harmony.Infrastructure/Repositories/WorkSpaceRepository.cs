using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Infrastructure.Repositories
{
    public class WorkSpaceRepository : IWorkspaceRepository
    {
        private readonly HarmonyContext _context;

        public WorkSpaceRepository(HarmonyContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(Workspace workspace)
        {
            await _context.Workspaces.AddAsync(workspace);

            return await _context.SaveChangesAsync();
        }

        public async Task<List<Workspace>> GetUserOwnedWorkspaces(string userId)
        {
            return await _context.Workspaces
                .Where(workspace => workspace.UserId == userId)
                .ToListAsync();
        }
    }
}
