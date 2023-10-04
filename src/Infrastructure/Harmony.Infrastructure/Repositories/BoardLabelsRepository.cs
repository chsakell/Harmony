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

        public async Task AddAsync(Label label)
        {
            await _context.Labels.AddAsync(label);
        }

        public async Task<int> CreateAsync(Label label)
        {
            await _context.Labels.AddAsync(label);

            return await _context.SaveChangesAsync();
        }
	}
}
