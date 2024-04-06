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
        Task AddAsync(Link link);
        Task<int> CreateAsync(Link link);
        Task<int> Update(Link link);
        Task<int> Delete(Link link);
    }
}
