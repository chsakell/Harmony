using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access Checklists
    /// </summary>
    public interface ICheckListRepository
    {
        Task<int> CreateAsync(CheckList checkList);
        Task<CheckList?> Get(Guid checklistId);
        Task<CheckList?> GetWithItems(Guid checklistId);
        Task<List<CheckList>> GetCardCheckLists(Guid cardId);
        Task<int> Update(CheckList checklist);
        Task<int> CountCardCheckLists(Guid cardId);
        Task<int> Delete(CheckList checklist);
        Task<Guid> GetBoardId(Guid checklistId);
    }
}
