using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    public interface ICardActivityRepository
    {
        IQueryable<CardActivity> Entities { get; }
        Task<List<CardActivity>> GetAsync(Guid cardId);
        Task<int> CreateAsync(CardActivity activity);
    }
}
