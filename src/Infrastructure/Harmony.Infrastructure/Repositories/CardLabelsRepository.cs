﻿using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class CardLabelRepository : ICardLabelRepository
    {
        private readonly HarmonyContext _context;

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