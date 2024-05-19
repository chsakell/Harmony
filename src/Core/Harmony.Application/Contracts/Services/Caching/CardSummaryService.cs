using Harmony.Application.Constants;
using Harmony.Application.DTO.Summaries;
using Harmony.Domain.Entities;
using Harmony.Domain.Extensions;
using MediatR;
using System.Text.Json;

namespace Harmony.Application.Contracts.Services.Caching
{
    public class CardSummaryService : ICardSummaryService
    {
        private readonly ICacheService _cache;

        public CardSummaryService(ICacheService cache)
        {
            _cache = cache;
        }

        public async Task SaveCardSummary(Guid boardId, CardSummary summary)
        {
            await _cache.HashHSetAsync(CacheKeys.ActiveCardSummaries(boardId),
                        summary.CardId.ToString(),
                        JsonSerializer.Serialize(summary,
                        CacheDomainExtensions._jsonSerializerOptions));
        }

        public async Task UpdateCardSummary(Guid boardId, Guid cardId, Action<CardSummary> updateFunc)
        {
            var cardSummary = await GetSummary(boardId, cardId);

            if (cardSummary != null)
            {
                updateFunc(cardSummary);

                await UpdateSummary(boardId, cardId, cardSummary);
            }
        }

        public async Task SaveCardSummaries(Guid boardId, Dictionary<Guid, CardSummary> summaries)
        {
            await _cache.HashMSetAsync(CacheKeys.ActiveCardSummaries(boardId), summaries, 
                CacheKeys.ActiveCardSummariesExpiration);
        }

        public async Task UpdateCardSummaries(Guid boardId, List<Guid> cardIds,
            Action<Dictionary<Guid, CardSummary>> updateFunc)
        {
            var cardSummaries = await GetSummaries(boardId, cardIds);

            if (cardSummaries != null && cardSummaries.Any())
            {
                updateFunc(cardSummaries);

                await _cache.HashMSetAsync
                    (CacheKeys.ActiveCardSummaries(boardId), cardSummaries);
            }
        }

        public async Task DeleteCardSummary(Guid boardId, Guid cardId)
        {
            await _cache.HashDeleleField(CacheKeys
                    .ActiveCardSummaries(boardId),
                        cardId.ToString());
        }

        private async Task<CardSummary> GetSummary(Guid boardId, Guid cardId)
        {
            return await _cache.HashGetAsync<CardSummary>(
                        CacheKeys.ActiveCardSummaries(boardId), cardId.ToString());
        }

        public async Task<Dictionary<Guid, CardSummary>> GetSummaries(Guid boardId, List<Guid> cardIds)
        {
            return await _cache.HashMGetFields<Guid, CardSummary>(
                        CacheKeys.ActiveCardSummaries(boardId),
                        cardIds.Select(c => c.ToString()).ToList());
        }

        public async Task<Dictionary<Guid, CardSummary>> GetAllSummaries(Guid boardId)
        {
            return await _cache.HashGetAllAsync<Guid, CardSummary>(
                        CacheKeys.ActiveCardSummaries(boardId));
        }

        private async Task UpdateSummary(Guid boardId, Guid cardId, CardSummary summary)
        {
            await _cache.HashHSetAsync(CacheKeys.ActiveCardSummaries(boardId),
                cardId.ToString(),
                JsonSerializer.Serialize(summary,
                CacheDomainExtensions._jsonSerializerOptions));
        }
    }
}
