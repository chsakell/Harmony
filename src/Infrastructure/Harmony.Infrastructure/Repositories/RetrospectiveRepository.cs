using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class RetrospectiveRepository : IRetrospectiveRepository
    {
        private readonly HarmonyContext _context;

        public RetrospectiveRepository(HarmonyContext context)
        {
            _context = context;
        }

        public IQueryable<Retrospective> Entities => _context.Set<Retrospective>();

        public async Task<Retrospective?> GetRetrospective(Guid id)
        {
            return await _context.Retrospectives
                .FirstOrDefaultAsync(Retrospective => Retrospective.Id == id);
        }

        public async Task<List<Retrospective>> GetRetrospectives(Guid boardId)
        {
            return await _context.Retrospectives
                .Where(r => r.BoardId == boardId)
                .ToListAsync();
        }

        public async Task Add(Retrospective retrospective)
        {
            await _context.Retrospectives.AddAsync(retrospective);
        }

        public async Task<int> Create(Retrospective retrospective)
        {
            await _context.Retrospectives.AddAsync(retrospective);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(Retrospective retrospective)
        {
            _context.Retrospectives.Update(retrospective);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(Retrospective retrospective)
        {
            _context.Retrospectives.Remove(retrospective);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> CountRetrospectives(Guid boardId)
        {
            return await _context.Retrospectives
                .Where(Retrospective => Retrospective.BoardId == boardId).CountAsync();
        }
    }
}
