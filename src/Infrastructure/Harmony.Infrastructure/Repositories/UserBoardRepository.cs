using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class UserBoardRepository : IUserBoardRepository
    {
        private readonly HarmonyContext _context;

        public UserBoardRepository(HarmonyContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(UserBoard Board)
        {
            await _context.UserBoards.AddAsync(Board);

            return await _context.SaveChangesAsync();
        }
	}
}
