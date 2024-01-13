using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly HarmonyContext _context;

        public AttachmentRepository(HarmonyContext context)
        {
            _context = context;
        }

        public IQueryable<Attachment> Entities => _context.Set<Attachment>();

        public async Task<int> CountAttachments(string userId)
        {
            return await _context.Attachments
                .Where(a => a.UserId == userId)
                .CountAsync();
        }
    }
}
