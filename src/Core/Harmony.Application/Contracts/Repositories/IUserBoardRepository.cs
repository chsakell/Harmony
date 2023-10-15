using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    public interface IUserBoardRepository
    {
        /// <summary>
        /// Create a User Board
        /// </summary>
        /// <param name="userBoard"></param>
        /// <returns></returns>
        Task<int> CreateAsync(UserBoard userBoard);
        Task<UserBoard?> GetUserBoard(Guid boardId, string userId);
        Task<int> CountBoardUsers(Guid boardId);
        Task<int> Delete(UserBoard userBoard);
        Task<List<UserBoardResponse>> GetBoardAccessMembers(Guid boardId);
        Task<UserBoardResponse?> GetBoardAccessMember(Guid boardId, string userId);

    }
}
