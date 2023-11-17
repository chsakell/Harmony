using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class IssueTypeRepository : IIssueTypeRepository
    {
        private readonly HarmonyContext _context;

        public IssueTypeRepository(HarmonyContext context)
        {
            _context = context;
        }

        public IQueryable<IssueType> Entities => _context.Set<IssueType>();

        public async Task<IssueType?> GetIssueType(Guid issueTypeId)
        {
            return await _context.IssueTypes
                .FirstOrDefaultAsync(IssueType => IssueType.Id == issueTypeId);
        }

        public async Task<List<IssueType>> GetIssueTypes(Guid boardId)
        {
            return await _context.IssueTypes
                .Where(l => l.BoardId == boardId)
                .ToListAsync();
        }

        public async Task AddAsync(IssueType IssueType)
        {
            await _context.IssueTypes.AddAsync(IssueType);
        }

        public async Task<int> CreateAsync(IssueType IssueType)
        {
            await _context.IssueTypes.AddAsync(IssueType);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(IssueType IssueType)
        {
            _context.IssueTypes.Update(IssueType);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(IssueType IssueType)
        {
            _context.IssueTypes.Remove(IssueType);

            return await _context.SaveChangesAsync();
        }
    }
}
