using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class CheckListRepository : ICheckListRepository
    {
        private readonly HarmonyContext _context;

        public CheckListRepository(HarmonyContext context)
        {
            _context = context;
        }

		public async Task<CheckList?> Get(Guid checklistId)
		{
			return await _context.CheckLists
                .FirstOrDefaultAsync(Checklist => Checklist.Id == checklistId);
		}

        public async Task<CheckList?> GetWithItems(Guid checklistId)
        {
            return await _context.CheckLists
                .Include(Checklist => Checklist.Items)
                .FirstOrDefaultAsync(Checklist => Checklist.Id == checklistId);
        }

        public async Task<Guid> GetBoardId(Guid checklistId)
        {
            return await _context.CheckLists
                .Include(c => c.Card)
                    .ThenInclude(c => c.BoardList)
                    .Where(c => c.Id == checklistId)
                    .Select(checkList => checkList.Card.BoardList.BoardId)
                .FirstOrDefaultAsync();
        }

        public async Task<CheckList?> GetWithCard(Guid ChecklistId)
        {
            return await _context.CheckLists.Include(c => c.Card)
                .FirstOrDefaultAsync(Checklist => Checklist.Id == ChecklistId);
        }

        public async Task<int> CreateAsync(CheckList checklist)
		{
			_context.CheckLists.Add(checklist);

			return await _context.SaveChangesAsync();
		}

		public async Task<int> Update(CheckList checklist)
		{
			_context.CheckLists.Update(checklist);

			return await _context.SaveChangesAsync();
		}

        public async Task<int> Delete(CheckList checklist)
        {
            _context.CheckLists.Remove(checklist);

            return await _context.SaveChangesAsync();
        }

        public async Task<List<CheckList>> GetCardCheckLists(Guid cardId)
        {
            return await _context.CheckLists
				.Where(checklist => checklist.CardId == cardId)
				.ToListAsync();
        }

        public async Task<int> CountCardCheckLists(Guid cardId)
        {
            return await _context.CheckLists
                .Where(checklist => checklist.CardId == cardId)
                .CountAsync();
        }
    }
}
