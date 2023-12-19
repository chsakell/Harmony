using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class SprintRepository : ISprintRepository
    {
        private readonly HarmonyContext _context;

        public SprintRepository(HarmonyContext context)
        {
            _context = context;
        }

        public IQueryable<Sprint> Entities => _context.Set<Sprint>();

        public async Task<Sprint?> GetSprint(Guid sprintId)
        {
            return await _context.Sprints
                .FirstOrDefaultAsync(Sprint => Sprint.Id == sprintId);
        }

        public async Task<List<Sprint>> GetSprints(Guid boardId)
        {
            return await _context.Sprints
                .Where(l => l.BoardId == boardId)
                .ToListAsync();
        }

        public async Task<List<Sprint>> GetActiveSprints(Guid boardId)
        {
            return await _context.Sprints
                .Where(s => s.BoardId == boardId && s.Status == Domain.Enums.SprintStatus.Active)
                .ToListAsync();
        }

        public async Task<List<Sprint>> GetNonCompletedSprints(Guid boardId)
        {
            return await _context.Sprints
                .Where(s => s.BoardId == boardId && s.Status != Domain.Enums.SprintStatus.Completed)
                .ToListAsync();
        }

        public async Task AddAsync(Sprint sprint)
        {
            await _context.Sprints.AddAsync(sprint);
        }

        public async Task<int> CreateAsync(Sprint sprint)
        {
            await _context.Sprints.AddAsync(sprint);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(Sprint sprint)
        {
            _context.Sprints.Update(sprint);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(Sprint sprint)
        {
            _context.Sprints.Remove(sprint);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> CountSprints(Guid boardId)
        {
            return await _context.Sprints
                .Where(sprint => sprint.BoardId == boardId).CountAsync();
        }

        public async Task<List<Sprint>> SearchSprints(Guid boardId, string term, int pageNumber, int pageSize,
            List<SprintStatus> statuses)
        {
            return await _context.Sprints
                .Where(s => s.BoardId == boardId && statuses.Contains(s.Status) &&
                    (string.IsNullOrEmpty(term) ? true : s.Name.Contains(term)))
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        }
    }
}
