using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access User Boards
    /// </summary>
    public interface IUserBoardRepository
    {
        Task AddAsync(UserBoard Board);
        Task<int> CreateAsync(UserBoard userBoard);
        Task<UserBoard?> GetUserBoard(Guid boardId, string userId);
        Task<int> CountBoardUsers(Guid boardId);
        Task<int> Delete(UserBoard userBoard);
        Task<List<UserBoardResponse>> GetBoardAccessMembers(Guid boardId);
        Task<UserBoardResponse?> GetBoardAccessMember(Guid boardId, string userId);
        Task<Workspace?> GetWorkspace(Guid boardId);
        Task<int> Update(UserBoard userBoard);
        IQueryable<Board> GetUserBoardsQuery(Guid? workspaceId, string userId);
        Task LoadBoardEntryAsync(UserBoard userBoard);
    }
}
