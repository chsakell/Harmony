using Harmony.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Contracts.Repositories
{
    public interface ICheckListItemRepository
    {
        Task<int> Add(CheckListItem item);
        Task<List<CheckListItem>> GetItems(Guid checklistId);
    }
}
