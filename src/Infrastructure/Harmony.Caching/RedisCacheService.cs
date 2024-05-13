using EasyCaching.Core;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Harmony.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IEasyCachingProvider _provider;
        private readonly IRedisCachingProvider _redisCachingProvider;

        public RedisCacheService(IEasyCachingProvider provider,
            IRedisCachingProvider redisCachingProvider)
        {
            _provider = provider;
            _redisCachingProvider = redisCachingProvider;
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

        #region Set

        public async Task<long> SetAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null)
        {
            return await _redisCachingProvider.SAddAsync(cacheKey, cacheValues, expiration);
        }

        public async Task<List<T>> SetMembersAsync<T>(string cacheKey)
        {
            return await _redisCachingProvider.SMembersAsync<T>(cacheKey);
        }


        #endregion

        #region Hash

        public async Task<Dictionary<I, T>> HashGetAllAsync<I, T>(string cacheKey)
        {
            var result = new Dictionary<I, T>();

            var hash = await _redisCachingProvider.HGetAllAsync(cacheKey);

            foreach (var kvp in hash)
            {
                var key = kvp.Key.Convert<I>();
                result[key] = JsonSerializer.Deserialize<T>(kvp.Value);
            }

            return result;
        }

        public async Task<bool> HashMSetAsync<I,T>(string cacheKey, Dictionary<I, T> vals, TimeSpan? expiration = null)
        {
            var result = new Dictionary<string, string>();

            foreach (var kvp in vals)
            {
                var key = kvp.Key.ToString();
                result[key] = JsonSerializer.Serialize(kvp.Value);
            }

            return await _redisCachingProvider.HMSetAsync(cacheKey, result, expiration);
        }


        #endregion


    }
}
