using EasyCaching.Core;
using Harmony.Application.Contracts.Services;
using Harmony.Domain.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace Harmony.Caching
{
    public class InMemoryCacheService : ICacheService
    {
        private readonly IEasyCachingProvider _provider;

        public InMemoryCacheService(IEasyCachingProvider provider)
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

        public Task<long> HashDeleleteField(string cacheKey, string field)
        {
            throw new NotImplementedException();
        }

        public Task<long> HashDeleleteFields(string cacheKey, IList<string> fields = null)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<I, T>> HashGetAllAsync<I, T>(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, string>> HashGetAllAsync(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public Task<T> HashGetAllAsync<T>(string cacheKey, Func<Dictionary<string, string>, T> converter)
        {
            throw new NotImplementedException();
        }

        public Task<T> HashGetAllOrCreateAsync<T>(string cacheKey, Func<Dictionary<string, string>, T> converter, Func<Task<T>> dataRetriever) where T : IHashable
        {
            throw new NotImplementedException();
        }

        public Task<T> HashGetAsync<T>(string cacheKey, string field)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HashHSetAsync(string cacheKey, string field, string value)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<I, T>> HashMGetFields<I, T>(string cacheKey, IList<string> fields)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<I, T>> HashMGetFields<I, T>(string cacheKey, List<string> fields)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HashMSetAsync<I, T>(string cacheKey, Dictionary<I, T> vals, TimeSpan? expiration = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HashMSetAsync(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveAsync<TItem>(string cacheKey,
            CancellationToken cancellationToken)
        {
            await _provider.RemoveAsync(cacheKey, cancellationToken);
        }

        public Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<long> SetAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> SetMembersAsync<T>(string cacheKey)
        {
            throw new NotImplementedException();
        }
    }
}
