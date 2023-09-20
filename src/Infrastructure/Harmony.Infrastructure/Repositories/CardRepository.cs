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
    public class CardRepository : ICardRepository
    {
        private readonly HarmonyContext _context;

        public CardRepository(HarmonyContext context)
        {
            _context = context;
        }

		public async Task<int> CountCards(Guid listId)
		{
			return await _context.Cards.Where(c => c.BoardListId == listId).CountAsync();
		}

		public async Task<int> Add(Card Card)
		{
			_context.Cards.Add(Card);

			return await _context.SaveChangesAsync();
		}
	}
}
