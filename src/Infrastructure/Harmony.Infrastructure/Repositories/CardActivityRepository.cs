using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class CardActivityRepository : ICardActivityRepository
    {
        private readonly HarmonyContext _context;

        public CardActivityRepository(HarmonyContext context)
        {
            _context = context;
        }

        public IQueryable<CardActivity> Entities => _context.Set<CardActivity>();

        public async Task<List<CardActivity>> GetAsync(Guid cardId)
        {
            return await _context.CardActivities.Where(ca => ca.CardId == cardId)
                .ToListAsync();
        }

        public async Task<int> CreateAsync(CardActivity activity)
        {
            await _context.CardActivities.AddAsync(activity);

            return await _context.SaveChangesAsync();
        }
    }
}
