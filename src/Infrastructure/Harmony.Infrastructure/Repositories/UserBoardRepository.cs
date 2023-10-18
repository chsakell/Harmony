using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
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

        public async Task<UserBoard?> GetUserBoard(Guid boardId, string userId)
        {
            return await _context.UserBoards
                .FirstOrDefaultAsync(ub => ub.BoardId == boardId && ub.UserId == userId);
        }

        public async Task AddAsync(UserBoard Board)
        {
            await _context.UserBoards.AddAsync(Board);
        }

        public async Task<int> CreateAsync(UserBoard Board)
        {
            await _context.UserBoards.AddAsync(Board);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> CountBoardUsers(Guid boardId)
        {
            return await _context.UserBoards
                .Where(userBoard => userBoard.BoardId == boardId)
                .CountAsync();
        }

        public async Task<List<UserBoardResponse>> GetBoardAccessMembers(Guid boardId)
        {
            var boardUsers = await (from userBoard in _context.UserBoards
                                    join user in _context.Users
                                    on userBoard.UserId equals user.Id
                                    where userBoard.BoardId == boardId
                                    select new UserBoardResponse
                                    {
                                        Id = user.Id,
                                        UserName = user.UserName,
                                        FirstName = user.FirstName,
                                        LastName = user.LastName,
                                        Email = user.Email,
                                        EmailConfirmed = user.EmailConfirmed,
                                        IsActive = user.IsActive,
                                        IsMember = true,
                                        PhoneNumber = user.PhoneNumber,
                                        Access = userBoard.Access
                                    })
                          .ToListAsync();

            return boardUsers;
        }

        public async Task<UserBoardResponse?> GetBoardAccessMember(Guid boardId, string userId)
        {
            var boardUser = await (from userBoard in _context.UserBoards
                                    join user in _context.Users
                                    on userBoard.UserId equals user.Id
                                    where userBoard.BoardId == boardId && user.Id == userId
                                    select new UserBoardResponse
                                    {
                                        Id = user.Id,
                                        UserName = user.UserName,
                                        FirstName = user.FirstName,
                                        LastName = user.LastName,
                                        Email = user.Email,
                                        EmailConfirmed = user.EmailConfirmed,
                                        IsActive = user.IsActive,
                                        IsMember = true,
                                        PhoneNumber = user.PhoneNumber,
                                        Access = userBoard.Access
                                    })
                          .FirstOrDefaultAsync();

            return boardUser;
        }

        public async Task<int> Delete(UserBoard userBoard)
        {
            _context.UserBoards.Remove(userBoard);

            return await _context.SaveChangesAsync();
        }
    }
}
