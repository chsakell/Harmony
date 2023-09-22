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
    public class BoardRepository : IBoardRepository
    {
        private readonly HarmonyContext _context;

        public BoardRepository(HarmonyContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(Board Board)
        {
            await _context.Boards.AddAsync(Board);

            return await _context.SaveChangesAsync();
        }

        public async Task<bool> Exists(Guid boardId)
        {
            return await _context.Boards
                .Where(b => b.Id == boardId)
                .CountAsync() > 0;
        }

        public async Task<Board> LoadBoard(Guid boardId)
        {
            return await _context.Boards
                .Include(b => b.Lists.Where(l => l.Status == Domain.Enums.BoardListStatus.Active))
                    .ThenInclude(l => l.Cards.Where(c => c.Status == Domain.Enums.CardStatus.Active))
                .Include(b => b.Users)
                .FirstAsync(board => board.Id == boardId);
        }

		public async Task<Board?> GetBoardWithLists(Guid boardId)
		{
            return await _context.Boards
                .Include (b => b.Lists)
                .FirstOrDefaultAsync(board => board.Id == boardId);
		}

		public async Task<int> Update(Board board)
		{
			_context.Boards.Update(board);

            return await _context.SaveChangesAsync();
		}
	}
}
