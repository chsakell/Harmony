using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access Lists
    /// </summary>
    public interface IBoardListRepository
    {
        IQueryable<BoardList> Entities { get; }
        Task<BoardList> Get(Guid boardListId);
        Task<List<BoardList>> GetBoardLists(Guid boardId);
        Task<int> CountActiveLists(Guid boardId);
        Task<int> CreateAsync(BoardList boardList);
        Task<int> Update(BoardList list);
        void UpdateEntry(BoardList list);
        Task<int> UpdateRange(List<BoardList> lists);
        Task<List<BoardList>> GetListsInPositionGreaterThan(Guid boardId, short position);
        Task<Dictionary<Guid, int>> GetTotalCardsForBoardLists(List<Guid> boardListIds);
    }
}
