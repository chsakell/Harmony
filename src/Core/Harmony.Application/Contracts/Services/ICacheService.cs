using Harmony.Application.Constants;
using Harmony.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Services
{
    public interface ICacheService
    {
        Task<TItem?> GetAsync<TItem>(string cacheKey,
            CancellationToken cancellationToken = default);

        Task<TItem?> GetOrCreateAsync<TItem>(string cacheKey,
            Func<Task<TItem>> dataRetriever, TimeSpan expiration,
            CancellationToken cancellationToken = default);

        Task RemoveAsync(string cacheKey,
            CancellationToken cancellationToken = default);
        Task<Dictionary<string, string>> HashGetAllAsync(string cacheKey);
        Task<Dictionary<I, T>> HashGetAllAsync<I, T>(string cacheKey);
        Task<Dictionary<I, T>> HashMGetFields<I, T>(string cacheKey, List<string> fields);
        Task<T> HashGetAllAsync<T>(string cacheKey,
            Func<Dictionary<string, string>, T> converter);
        Task<T> HashGetAllOrCreateAsync<T>(string cacheKey,
            Func<Dictionary<string, string>, T> converter,
            Func<Task<T>> dataRetriever, TimeSpan? expiration = null) where T : IHashable;

        Task<T> HashGetAsync<T>(string cacheKey, string field);
        Task<bool> HashMSetAsync<I, T>(string cacheKey, Dictionary<I, T> vals, TimeSpan? expiration = null);
        Task<bool> HashMSetAsync(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null);
        Task<bool> HashHSetAsync(string cacheKey, string field, string value);
        Task<long> HashDeleleFields(string cacheKey, IList<string> fields = null);
        Task<long> HashDeleleField(string cacheKey, string field);
    }
}
