using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access Cards
    /// </summary>
    public interface ICardRepository
    {
        IQueryable<Card> Entities { get; }
        Task<Card> Get(Guid cardId);
        Task<Guid> GetBoardId(Guid cardId);
        Task<Card> Load(Guid cardId);
        Task<Card> GetByPosition(Guid boardListId, short position);
		Task<List<Card>> GetCardsInPositionGreaterThan(Guid boardListId, short position);
		Task<List<Card>> GetCardsInPositionGreaterOrEqualThan(Guid boardListId, short position);
		Task<int> CountCards(Guid listId);
        Task<int> CreateAsync(Card card);
		Task<int> Update(Card card);
		void UpdateEntry(Card Card);
        Task<int> GetNextSerialNumber(Guid boardId);
        Task<int> CountBacklogCards(Guid boardId);
    }
}
