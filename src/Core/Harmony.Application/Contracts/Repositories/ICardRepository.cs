using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    public interface ICardRepository
    {
        IQueryable<Card> Entities { get; }
        Task<Card> Get(Guid cardId);
        Task<Guid> GetBoardId(Guid cardId);
        Task<Card> Load(Guid cardId);
        Task<Card> GetByPosition(Guid boardListId, byte position);
		Task<List<Card>> GetCardsInPositionGreaterThan(Guid boardListId, byte position);
		Task<List<Card>> GetCardsInPositionGreaterOrEqualThan(Guid boardListId, byte position);
		Task<int> CountCards(Guid listId);
        Task<int> Add(Card card);
		Task<int> Update(Card card);
		void UpdateEntry(Card Card);
	}
}
