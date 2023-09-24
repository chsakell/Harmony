using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly HarmonyContext _context;

        public CardRepository(HarmonyContext context)
        {
            _context = context;
        }

		public async Task<Card?> Get(Guid cardId)
		{
			return await _context.Cards.FirstOrDefaultAsync(card => card.Id == cardId);
		}

        public async Task<Card?> Load(Guid cardId)
        {
            return await _context.Cards
				.Include(card => card.BoardList)
				.Include(card => card.CheckLists)
				.Include(card => card.Comments)
				.Include(card => card.Activities)
				.Include(card => card.Attachments)
				.FirstOrDefaultAsync(card => card.Id == cardId);
        }

        public async Task<Card?> GetByPosition(Guid boardListId, byte position)
		{
			return await _context.Cards
				.FirstOrDefaultAsync(card => card.BoardListId == boardListId && card.Position == position);
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

		public async Task<int> Update(Card Card)
		{
			_context.Cards.Update(Card);

			return await _context.SaveChangesAsync();
		}

		public void UpdateEntry(Card Card)
		{
			_context.Cards.Update(Card);
		}

		public async Task<List<Card>> GetCardsInPositionGreaterThan(Guid boardListId, byte position)
		{
			return await _context.Cards.Where(c => c.BoardListId == boardListId && c.Position > position).ToListAsync();
		}

		public async Task<List<Card>> GetCardsInPositionGreaterOrEqualThan(Guid boardListId, byte position)
		{
			return await _context.Cards.Where(c => c.BoardListId == boardListId && c.Position >= position).ToListAsync();
		}
	}
}
