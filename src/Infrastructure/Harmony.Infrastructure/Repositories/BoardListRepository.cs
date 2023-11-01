using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class BoardListRepository : IBoardListRepository
    {
        private readonly HarmonyContext _context;

        public BoardListRepository(HarmonyContext context)
        {
            _context = context;
        }

        public IQueryable<BoardList> Entities => _context.Set<BoardList>();

        public async Task<int> CountActiveLists(Guid boardId)
		{
			return await _context.BoardLists
                .Where(bl => bl.BoardId == boardId 
                && bl.Status == Domain.Enums.BoardListStatus.Active).CountAsync();
		}

		public async Task<int> CreateAsync(BoardList boardList)
		{
			_context.BoardLists.Add(boardList);

			return await _context.SaveChangesAsync();
		}

        public async Task<BoardList?> Get(Guid boardListId)
        {
            return await _context.BoardLists.FirstOrDefaultAsync(l => l.Id == boardListId);
        }

        public async Task<List<BoardList>> GetBoardLists(Guid boardId)
        {
            return await _context.BoardLists
                .Where(l => l.BoardId == boardId 
                    && l.Status == Domain.Enums.BoardListStatus.Active)
                .ToListAsync();
        }

        public async Task<Dictionary<Guid, int>> GetTotalCardsForBoardLists(List<Guid> boardListIds)
        {
            var result = new Dictionary<Guid, int>();
            var totalCardsPerList = await _context.BoardLists
                    .Include(bl => bl.Cards.Where(c => c.Status == Domain.Enums.CardStatus.Active))
                .Where(l => boardListIds.Contains(l.Id)
                    && l.Status == Domain.Enums.BoardListStatus.Active)
                .GroupBy(list => list.Id)
                .Select(group => new 
                    { 
                        Id = group.Key, 
                        TotalCards = group.FirstOrDefault().Cards.Count 
                    })
                .ToListAsync();

            foreach (var group in totalCardsPerList)
            {
                result[group.Id] = group.TotalCards;
            }

            return result;
        }

        public void UpdateEntry(BoardList list)
        {
            _context.BoardLists.Update(list);
        }

        public async Task<int> Update(BoardList list)
        {
            _context.BoardLists.Update(list);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateRange(List<BoardList> lists)
        {
            _context.BoardLists.UpdateRange(lists);

            return await _context.SaveChangesAsync();
        }

        public async Task<List<BoardList>> GetListsInPositionGreaterThan(Guid boardId, short position)
        {
            return await _context.BoardLists
                .Where(l => l.BoardId == boardId && l.Position > position 
                    && l.Status == Domain.Enums.BoardListStatus.Active).ToListAsync();
        }
    }
}
