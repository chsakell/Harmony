using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
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

        public IQueryable<Card> Entities => _context.Set<Card>();

        public async Task<Card?> Get(Guid cardId)
		{
			return await _context.Cards.FirstOrDefaultAsync(card => card.Id == cardId);
		}

        public async Task<Card?> GetWithBoardList(Guid cardId)
        {
            return await _context.Cards.Include(c => c.BoardList)
                .FirstOrDefaultAsync(card => card.Id == cardId);
        }

        public async Task<Guid> GetBoardId(Guid cardId)
        {
			var card = (await _context.Cards
					.Include(c => c.BoardList)
				.FirstOrDefaultAsync(c => c.Id == cardId));

            if(card.BoardList == null && card.Status == CardStatus.Backlog)
            {
                await LoadIssueEntryAsync(card);

                return card.IssueType.BoardId;
            }

			return card.BoardList.BoardId;
        }

        public async Task<Card?> Load(Guid cardId)
        {
            return await _context.Cards
				.Include(card => card.BoardList)
					.ThenInclude(bl => bl.Board)
				.Include(card => card.CheckLists)
					.ThenInclude(list => list.Items)
				.Include(card => card.Comments)
				.Include(card => card.Attachments)
				.Include(card => card.Members)
				.Include(card => card.Labels)
					.ThenInclude(cl => cl.Label)
				.Include(card => card.Sprint)
                .Include(card => card.IssueType)
				.FirstOrDefaultAsync(card => card.Id == cardId);
        }

        public async Task<Card?> GetByPosition(Guid? boardListId, short position, CardStatus status)
		{
			return await _context.Cards
				.FirstOrDefaultAsync(card => card.BoardListId == boardListId 
				&& card.Position == position && card.Status == status);
		}

		public async Task<int> CountCards(Guid listId)
		{
			return await _context.Cards.Where(c => c.BoardListId == listId).CountAsync();
		}

        public async Task<int> CountBacklogCards(Guid boardId)
        {
            return await _context.Cards
					.Include(c => c.IssueType)
				.Where(c => c.IssueType.BoardId == boardId 
				&& c.Status == Domain.Enums.CardStatus.Backlog).CountAsync();
        }

        public async Task<int> GetNextBacklogPosition(Guid boardId)
        {
            var positions = await _context.Cards
                    .Include(c => c.IssueType)
                .Where(c => c.IssueType.BoardId == boardId
                && c.Status == Domain.Enums.CardStatus.Backlog)
				.Select(c => c.Position).ToListAsync();

			return !positions.Any() ? 1 : positions.Max() + 1;
        }

        public async Task<int> CountBoardCards(Guid boardId)
        {
            return await _context.Cards
                    .Include(c => c.BoardList)
					.Include(c => c.IssueType)
                .Where(c => c.BoardList.BoardId == boardId || c.IssueType.BoardId == boardId)
				.Select(c => c.Id)
				.CountAsync();
        }

        public async Task<int> GetNextSerialNumber(Guid boardId)
        {
			var totalCards = await CountBoardCards(boardId);
            return totalCards + 1;
        }

        public async Task LoadIssueEntryAsync(Card Card)
        {
            await _context.Entry(Card).Reference(card => card.IssueType).LoadAsync();
        }

        public async Task LoadBoardListEntryAsync(Card Card)
        {
            await _context.Entry(Card).Reference(card => card.BoardList).LoadAsync();
        }

        public async Task<int> CreateAsync(Card Card)
		{
			_context.Cards.Add(Card);

			return await _context.SaveChangesAsync();
		}

		public async Task<int> Update(Card Card)
		{
			_context.Cards.Update(Card);

			return await _context.SaveChangesAsync();
		}

        public async Task<int> UpdateRange(List<Card> cards)
        {
            _context.Cards.UpdateRange(cards);

            return await _context.SaveChangesAsync();
        }

        public void UpdateEntry(Card Card)
		{
			_context.Cards.Update(Card);
		}

		public async Task<List<Card>> GetCardsInPositionGreaterThan(Guid boardListId, short position)
		{
			return await _context.Cards.Where(c => c.BoardListId == boardListId && c.Position > position).ToListAsync();
		}

		public async Task<List<Card>> GetCardsInPositionGreaterOrEqualThan(Guid boardListId, short position)
		{
			return await _context.Cards.Where(c => c.BoardListId == boardListId && c.Position >= position).ToListAsync();
		}

		public async Task<List<Card>> GetPendingSprintCards(Guid sprintId)
		{
            return await _context.Cards
				.Include(c => c.BoardList)
				.Where(c => c.BoardList.CardStatus != BoardListCardStatus.DONE && c.SprintId == sprintId)
				.ToListAsync();
        }
    }
}
