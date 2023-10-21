using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    public interface IBoardListRepository
    {
        Task<BoardList> Get(Guid boardListId);
        Task<int> CountLists(Guid boardId);
        Task<int> Add(BoardList boardList);
        Task<int> Update(BoardList list);
        void UpdateEntry(BoardList list);
        Task<List<BoardList>> GetListsInPositionGreaterThan(Guid boardId, short position);
    }
}
