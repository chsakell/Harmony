using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    public interface IBoardListRepository
    {
        Task<BoardList> Get(Guid boardListId);
        Task<List<BoardList>> GetBoardLists(Guid boardId);
        Task<int> CountActiveLists(Guid boardId);
        Task<int> Add(BoardList boardList);
        Task<int> Update(BoardList list);
        void UpdateEntry(BoardList list);
        Task<int> UpdateRange(List<BoardList> lists);
        Task<List<BoardList>> GetListsInPositionGreaterThan(Guid boardId, short position);
    }
}
