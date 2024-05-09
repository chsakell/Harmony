using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access links
    /// </summary>
    public interface ILinkRepository
    {
        IQueryable<Link> Entities { get; }
        Task<Link?> GetLink(Guid linkId);
        Task<Link?> GetLink(Guid sourceCardId, Guid targetCardId, LinkType type);
        Task AddAsync(Link link);
        Task<int> CreateAsync(Link link);
        Task<int> Update(Link link);
        void Remove(Link link);
        Task<int> Delete(Link link);
        Task<int> GetTotalLinks(Guid cardId);
    }
}
