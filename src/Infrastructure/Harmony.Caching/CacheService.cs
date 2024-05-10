using EasyCaching.Core;
using Harmony.Application.Contracts.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Harmony.Caching
{
    public class CacheService : ICacheService
    {
        private readonly IEasyCachingProvider _provider;

        public CacheService(IEasyCachingProvider provider)
        {
            _provider = provider;
        }

        public async Task<TItem?> GetAsync<TItem>(string cacheKey,
            CancellationToken cancellationToken = default)
        {
            var cacheValue = await _provider
                .GetAsync<TItem>(cacheKey, cancellationToken);

            return cacheValue.Value;
        }

        public async Task<TItem?> GetOrCreateAsync<TItem>(string cacheKey, 
            Func<Task<TItem>> dataRetriever, TimeSpan expiration,
            CancellationToken cancellationToken)
        {
            var cacheValue = await _provider
                .GetAsync(cacheKey, dataRetriever, expiration, cancellationToken);

            return cacheValue.Value;
        }

        public async Task RemoveAsync<TItem>(string cacheKey,
            CancellationToken cancellationToken)
        {
            await _provider.RemoveAsync(cacheKey, cancellationToken);
        }
    }
}
