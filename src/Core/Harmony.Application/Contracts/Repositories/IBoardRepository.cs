using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access Boards
    /// </summary>
    public interface IBoardRepository
    {
        IQueryable<Board> Entities { get; }
        Task<Board> GetAsync(Guid boardId);
        Task<int> CreateAsync(Board Board);
        Task AddAsync(Board Board);
        Task<Board> GetBoardWithLists(Guid boardId);
        Task<Board> LoadBoard(Guid boardId, int maxCardsPerList);
        Task<Board> LoadBoardList(Guid boardId, Guid listId, int page, int maxCardsPerList);
        Task<bool> Exists(Guid boardId);
        Task<bool> Exists(Guid workspaceId, string key);
        Task<int> Update(Board board);
        Task LoadWorkspaceEntryAsync(Board board);
        Task LoadBoardListEntryAsync(Board board);
    }
}
