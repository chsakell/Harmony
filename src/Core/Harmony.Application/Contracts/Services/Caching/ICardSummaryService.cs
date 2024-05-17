using Harmony.Application.DTO.Summaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Services.Caching
{
    public interface ICardSummaryService
    {
        Task SaveCardSummary(Guid boardId, CardSummary summary);
        Task UpdateCardSummary(Guid boardId, Guid cardId, Action<CardSummary> updateFunc);
        Task UpdateCardSummaries(Guid boardId, List<Guid> cardIds,
            Action<Dictionary<Guid, CardSummary>> updateFunc);
        Task DeleteCardSummary(Guid boardId, Guid cardId);
    }
}
