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

		public async Task<int> CountLists(Guid boardId)
		{
			return await _context.BoardLists.Where(bl => bl.BoardId == boardId).CountAsync();
		}

		public async Task<int> Add(BoardList boardList)
		{
			_context.BoardLists.Add(boardList);

			return await _context.SaveChangesAsync();
		}

        public async Task<BoardList?> Get(Guid boardListId)
        {
            return await _context.BoardLists.FirstOrDefaultAsync(l => l.Id == boardListId);
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

        public async Task<List<BoardList>> GetListsInPositionGreaterThan(Guid boardId, short position)
        {
            return await _context.BoardLists
                .Where(l => l.BoardId == boardId && l.Position > position 
                    && l.Status == Domain.Enums.BoardListStatus.Active).ToListAsync();
        }
    }
}
