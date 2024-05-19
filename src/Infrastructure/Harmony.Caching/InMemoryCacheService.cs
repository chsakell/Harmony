using EasyCaching.Core;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Extensions;
using Harmony.Domain.Contracts;
using Harmony.Domain.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.Collections.Concurrent;
using System.Text.Json;

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

        public async Task<long> HashDeleleField(string cacheKey, string field)
        {
            return await HashDeleleFields(cacheKey, new List<string> { field });
        }

        public async Task<long> HashDeleleFields(string cacheKey, IList<string> fields = null)
        {
            var dictionary = await _provider.GetAsync<Dictionary<string, string>>(cacheKey);

            if(dictionary == null || !dictionary.HasValue)
            {
                return default;
            }

            var dictVal = dictionary.Value;

            foreach(var field in fields)
            {
                dictVal.Remove(field);
            }

            await HashMSetAsync(cacheKey, dictVal);

            return default;
        }

        public async Task<Dictionary<I, T>> HashGetAllAsync<I, T>(string cacheKey)
        {
            var result = new Dictionary<I, T>();

            var hash = await HashGetAllAsync(cacheKey);

            if(hash == null)
            {
                return result;
            }

            foreach (var kvp in hash)
            {
                var key = kvp.Key.Convert<I>();
                result[key] = JsonSerializer.Deserialize<T>(kvp.Value);
            }

            return result;
        }

        public async Task<Dictionary<string, string>> HashGetAllAsync(string cacheKey)
        {
            var dictionary = await _provider.GetAsync<Dictionary<string, string>>(cacheKey);

            return dictionary.HasValue ? dictionary.Value : default;
        }

        public async Task<T> HashGetAllAsync<T>(string cacheKey, Func<Dictionary<string, string>, T> converter)
        {
            var dictionary = await HashGetAllAsync(cacheKey);

            return converter(dictionary);
        }

        public async Task<T> HashGetAllOrCreateAsync<T>(string cacheKey, Func<Dictionary<string, string>, T> converter,
            Func<Task<T>> dataRetriever, TimeSpan? expiration = null) where T : IHashable
        {
            var dictionary = await _provider.GetAsync<Dictionary<string, string>>(cacheKey);

            if (dictionary == null || !dictionary.HasValue || !dictionary.Value.Keys.Any())
            {
                var data = await dataRetriever();
                var hashTable = data.ConvertToDictionary();

                await HashMSetAsync(cacheKey, hashTable, expiration);

                return converter(hashTable);
            }

            return converter(dictionary.Value);
        }

        public async Task<T> HashGetAsync<T>(string cacheKey, string field)
        {
            var dictionary = await _provider.GetAsync<Dictionary<string, string>>(cacheKey);

            if (dictionary == null || !dictionary.HasValue || !dictionary.Value.TryGetValue(field, out var fieldDictionary))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(fieldDictionary, CacheDomainExtensions._jsonSerializerOptions);
        }

        public async Task<bool> HashHSetAsync(string cacheKey, string field, string value)
        {
            var dictionary = await _provider.GetAsync<Dictionary<string, string>>(cacheKey);

            dictionary.Value[field] = value;

            await HashMSetAsync(cacheKey, dictionary.Value);

            return true;
        }

        public async Task<Dictionary<I, T>> HashMGetFields<I, T>(string cacheKey, List<string> fields)
        {
            var result = new Dictionary<I, T>();

            var dictionary = await _provider.GetAsync<Dictionary<string, string>>(cacheKey);

            var hash = dictionary.Value;

            foreach (var kvp in hash)
            {
                if(!fields.Contains(kvp.Key))
                {
                    continue;
                }

                var key = kvp.Key.Convert<I>();
                result[key] = JsonSerializer.Deserialize<T>(kvp.Value);
            }

            return result;
        }

        public async Task<bool> HashMSetAsync<I, T>(string cacheKey, Dictionary<I, T> vals, TimeSpan? expiration = null)
        {
            var dictionary = await _provider.GetAsync<Dictionary<string, string>>(cacheKey);

            var result = dictionary.Value ?? new Dictionary<string, string>();

            foreach (var kvp in vals)
            {
                var key = kvp.Key.ToString();
                result[key] = JsonSerializer.Serialize(kvp.Value);
            }

            await _provider.SetAsync(cacheKey, result, expiration ?? TimeSpan.FromHours(1));

            return true;
        }

        public async Task<bool> HashMSetAsync(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null)
        {
            await _provider.SetAsync(cacheKey, vals, expiration ?? TimeSpan.FromHours(1));

            return true;
        }

        public async Task RemoveAsync<TItem>(string cacheKey,
            CancellationToken cancellationToken)
        {
            await _provider.RemoveAsync(cacheKey, cancellationToken);
        }

        public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            await _provider.RemoveAsync(cacheKey, cancellationToken);
        }
    }
}
