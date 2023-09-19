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

        public async Task<List<Board>> GetUserOwnedBoards(string userId)
        {
            return await _context.Boards
                .Where(Board => Board.UserId == userId)
                .ToListAsync();
        }
    }
}
