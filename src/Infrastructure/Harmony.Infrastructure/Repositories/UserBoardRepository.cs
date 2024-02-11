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
                                        Access = userBoard.Access,
                                        ProfilePicture = user.ProfilePicture
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

        public async Task<Workspace?> GetWorkspace(Guid boardId)
        {
            return await _context.UserBoards
                .Include(ub => ub.Board)
                .ThenInclude(b => b.Workspace)
                .Where(ub => ub.BoardId == boardId)
                .Select(ub => ub.Board.Workspace)
                .FirstOrDefaultAsync();
        }

        public async Task<int> Delete(UserBoard userBoard)
        {
            _context.UserBoards.Remove(userBoard);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(UserBoard userboard)
        {
            _context.UserBoards.Update(userboard);

            return await _context.SaveChangesAsync();
        }

        public IQueryable<Board> GetUserBoardsQuery(Guid? workspaceId, string userId)
        {
            var query = from UserBoard userBoard in _context.UserBoards
                        join board in _context.Boards on userBoard.BoardId equals board.Id
                        join workspace in _context.Workspaces on board.WorkspaceId equals workspace.Id
                        where userBoard.UserId == userId && 
                        workspace.Id == (workspaceId ?? workspace.Id) 
                            && (board.Visibility == Domain.Enums.BoardVisibility.Private || board.Visibility == Domain.Enums.BoardVisibility.Public)
                        select board;

            return query;
        }

        public async Task LoadBoardEntryAsync(UserBoard userBoard)
        {
            await _context.Entry(userBoard).Reference(ub => ub.Board).LoadAsync();
        }
    }
}
