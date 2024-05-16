using EasyCaching.Core;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Extensions;
using Harmony.Domain.Contracts;
using Harmony.Domain.Extensions;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
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

        public async Task RemoveAsync(string cacheKey,
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

        public async Task<Dictionary<I, T>> HashMGetFields<I,T>(string cacheKey, List<string> fields)
        {
            var result = new Dictionary<I, T>();

            var hash = await _redisCachingProvider.HMGetAsync(cacheKey, fields);

            foreach (var kvp in hash)
            {
                var key = kvp.Key.Convert<I>();
                result[key] = JsonSerializer.Deserialize<T>(kvp.Value);
            }

            return result;
        }

        public async Task<Dictionary<string, string>> HashGetAllAsync(string cacheKey)
        {
            return await _redisCachingProvider.HGetAllAsync(cacheKey);
        }

        public async Task<T> HashGetAllAsync<T>(string cacheKey, 
            Func<Dictionary<string, string>, T> converter)
        {
            var dictionary = await _redisCachingProvider.HGetAllAsync(cacheKey);

            return converter(dictionary);
        }

        public async Task<T> HashGetAllOrCreateAsync<T>(string cacheKey,
            Func<Dictionary<string, string>, T> converter,
            Func<Task<T>> dataRetriever) where T: IHashable
        {
            var dictionary = await _redisCachingProvider.HGetAllAsync(cacheKey);

            if(dictionary == null || !dictionary.Keys.Any())
            {
                var data = await dataRetriever();
                var hashTable = data.ConvertToDictionary();

                await HashMSetAsync(cacheKey, hashTable);

                return converter(hashTable);
            }

            return converter(dictionary);
        }

        public async Task<T> HashGetAsync<T>(string cacheKey, string field)
        {
            var value = await _redisCachingProvider.HGetAsync(cacheKey, field);

            if(value == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value, CacheDomainExtensions._jsonSerializerOptions);
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

        public async Task<bool> HashMSetAsync(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null)
        {
            return await _redisCachingProvider.HMSetAsync(cacheKey, vals, expiration);
        }

        public async Task<bool> HashHSetAsync(string cacheKey, string field, string value)
        {
            return await _redisCachingProvider.HSetAsync(cacheKey, field, value);
        }

        public async Task<long> HashDeleleteFields(string cacheKey, IList<string> fields = null)
        {
            return await _redisCachingProvider.HDelAsync(cacheKey, fields);
        }

        public async Task<long> HashDeleleteField(string cacheKey, string field)
        {
            return await HashDeleleteFields(cacheKey, new List<string> { field });
        }

        #endregion


    }
}
