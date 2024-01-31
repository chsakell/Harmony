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
        Task<Card?> GetWithBoardList(Guid cardId);
        Task<Guid> GetBoardId(Guid cardId);
        Task<Card> Load(Guid cardId);
		Task<List<Card>> GetCardsInPositionGreaterThan(Guid boardListId, short position);
		Task<List<Card>> GetCardsInPositionGreaterOrEqualThan(Guid boardListId, short position);
        Task<List<Card>> GetPendingSprintCards(Guid sprintId);
        Task<int> CountCards(Guid listId);
        Task<int> CountActiveCards(Guid listId);
        Task<short> GetMaxActivePosition(Guid listId);
        Task<int> CreateAsync(Card card);
		Task<int> Update(Card card);
        Task<int> UpdateRange(List<Card> cards);

        Task LoadBoardListEntryAsync(Card Card);
        Task LoadIssueEntryAsync(Card Card);
        void UpdateEntry(Card Card);
        Task<int> GetNextSerialNumber(Guid boardId);
        Task<int> CountBacklogCards(Guid boardId);
        Task<int> GetNextBacklogPosition(Guid boardId);
        Task<int> CountBoardCards(Guid boardId);
    }
}
