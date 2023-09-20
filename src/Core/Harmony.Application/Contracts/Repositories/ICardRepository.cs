using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Repositories
{
    public interface ICardRepository
    {
		Task<Card> Get(Guid cardId);
		Task<Card> GetByPosition(Guid boardListId, byte position);
		Task<List<Card>> GetCardsInPositionGreaterThan(Guid boardListId, byte position);
		Task<List<Card>> GetCardsInPositionGreaterOrEqualThan(Guid boardListId, byte position);
		Task<int> CountCards(Guid listId);
        Task<int> Add(Card card);
		Task<int> Update(Card card);
		void UpdateEntry(Card Card);
	}
}
