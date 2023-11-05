using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access CheckLists items
    /// </summary>
    public interface ICheckListItemRepository
    {
        Task<int> CreateAsync(CheckListItem item);
        Task<CheckListItem> Get(Guid checklistItemId);
        Task<List<CheckListItem>> GetItems(Guid checklistId);
        Task<int> Update(CheckListItem checklistItem);
    }
}
