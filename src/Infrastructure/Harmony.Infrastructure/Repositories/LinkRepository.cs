using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class LinkRepository : ILinkRepository
    {
        private readonly HarmonyContext _context;

        public LinkRepository(HarmonyContext context)
        {
            _context = context;
        }

        public IQueryable<Link> Entities => _context.Set<Link>();

        public async Task<Link?> GetLink(Guid linkId)
        {
            return await _context.Links
                .FirstOrDefaultAsync(Link => Link.Id == linkId);
        }

        public async Task<Link?> GetLink(Guid sourceCardId, Guid targetCardId, LinkType type)
        {
            return await _context.Links
                .FirstOrDefaultAsync(link => link.SourceCardId == sourceCardId &&
                link.TargetCardId == targetCardId && link.Type == type);
        }

        public async Task AddAsync(Link link)
        {
            await _context.Links.AddAsync(link);
        }

        public async Task<int> CreateAsync(Link link)
        {
            await _context.Links.AddAsync(link);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(Link link)
        {
            _context.Links.Update(link);

            return await _context.SaveChangesAsync();
        }

        public void Remove(Link link)
        {
            _context.Links.Remove(link);
        }

        public async Task<int> Delete(Link link)
        {
            _context.Links.Remove(link);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalLinks(Guid cardId)
        {
            return await _context.Links.Where(l => l.SourceCardId == cardId).CountAsync();
        }

        public async Task<Dictionary<Guid, int>> GetTotalLinks(List<Guid> cardIds)
        {
            return await _context.Links
                .Where(l => cardIds.Contains(l.SourceCardId))
                .GroupBy(l => l.SourceCardId)
                .Select(g => new
                {
                    CardId = g.Key,
                    TotalLinks = g.Count()
                })
            .ToDictionaryAsync(g => g.CardId, g => g.TotalLinks);
        }
    }
}
