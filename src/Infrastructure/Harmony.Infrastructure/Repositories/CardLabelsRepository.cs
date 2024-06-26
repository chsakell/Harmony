﻿using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class CardLabelRepository : ICardLabelRepository
    {
        private readonly HarmonyContext _context;

        public IQueryable<CardLabel> Entities => _context.Set<CardLabel>();

        public CardLabelRepository(HarmonyContext context)
        {
            _context = context;
        }

        public async Task<List<CardLabel>> GetLabels(Guid cardId)
        {
            return await _context.CardLabels
                    .Include(cl => cl.Label)
                .Where(cl => cl.CardId == cardId)
                .ToListAsync();
        }

        public async Task<CardLabel?> GetLabel(Guid cardId, Guid labelId)
        {
            return await _context.CardLabels
                .Where(cl => cl.CardId == cardId && cl.LabelId == labelId)
                .FirstOrDefaultAsync();
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

        public async Task<int> CreateCardLabelAsync(CardLabel label)
        {
            await _context.CardLabels.AddAsync(label);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteCardLabel(CardLabel label)
        {
            _context.CardLabels.Remove(label);

            return await _context.SaveChangesAsync();
        }

        public async Task<Label?> GetLabel(Guid labelId)
        {
            return await _context.Labels
                .Where(l => l.Id == labelId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Guid>> GetLabelIds(Guid cardId)
        {
            return await _context.CardLabels
                .Where(cl => cl.CardId == cardId)
                .Select(l => l.LabelId)
                .ToListAsync();
        }

        public async Task<Dictionary<Guid, IEnumerable<Guid>>> GetLabelIds(List<Guid> cardIds)
        {
            return await _context.CardLabels
                .Where(cl => cardIds.Contains(cl.CardId))
                .GroupBy(c => c.CardId)
                .Select(cl => new
                {
                    CardId = cl.Key,
                    Labels = cl.ToList().Select(l => l.LabelId)
                })
                .ToDictionaryAsync(c => c.CardId, c => c.Labels);
        }
    }
}
