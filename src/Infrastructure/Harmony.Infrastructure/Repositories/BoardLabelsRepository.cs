using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class BoardLabelRepository : IBoardLabelRepository
    {
        private readonly HarmonyContext _context;

        public BoardLabelRepository(HarmonyContext context)
        {
            _context = context;
        }

        public async Task<Label?> GetLabel(Guid labelId)
        {
            return await _context.Labels
                .FirstOrDefaultAsync(label => label.Id == labelId);
        }

        public async Task<List<Label>> GetLabels(Guid boardId)
        {
            return await _context.Labels
                .Where(l => l.BoardId == boardId)
                .ToListAsync();
        }

        public async Task AddAsync(Label label)
        {
            await _context.Labels.AddAsync(label);
        }

        public async Task<int> CreateAsync(Label label)
        {
            await _context.Labels.AddAsync(label);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(Label label)
        {
            _context.Labels.Update(label);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(Label label)
        {
            _context.Labels.Remove(label);

            return await _context.SaveChangesAsync();
        }
    }
}
