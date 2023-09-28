using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    public interface IBoardRepository
    {
        /// <summary>
        /// Create a Board
        /// </summary>
        /// <param name="Board"></param>
        /// <returns></returns>
        Task<int> CreateAsync(Board Board);
        Task AddAsync(Board Board);
        Task<Board> GetBoardWithLists(Guid boardId);
        Task<Board> LoadBoard(Guid boardId);
        Task<bool> Exists(Guid boardId);
        Task<int> Update(Board board);

	}
}
