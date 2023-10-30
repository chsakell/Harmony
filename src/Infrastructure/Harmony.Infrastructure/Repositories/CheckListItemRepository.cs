using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class CheckListItemRepository : ICheckListItemRepository
    {
        private readonly HarmonyContext _context;

        public CheckListItemRepository(HarmonyContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(CheckListItem item)
        {
            _context.CheckListItems.Add(item);

            return await _context.SaveChangesAsync();
        }

        public async Task<CheckListItem?> Get(Guid checklistItemId)
        {
            return await _context.CheckListItems
                .FirstOrDefaultAsync(item => item.Id == checklistItemId);
        }

        public async Task<List<CheckListItem>> GetItems(Guid checklistId)
        {
            return await _context.CheckListItems
                .Where(item => item.CheckListId == checklistId)
                .ToListAsync();
        }

        public async Task<int> Update(CheckListItem checklistItem)
        {
            _context.CheckListItems.Update(checklistItem);

            return await _context.SaveChangesAsync();
        }
    }
}
