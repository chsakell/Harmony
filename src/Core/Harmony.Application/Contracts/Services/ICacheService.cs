using Harmony.Application.Constants;
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

        Task<List<T>> SetMembersAsync<T>(string cacheKey);

        Task<TItem?> GetOrCreateAsync<TItem>(string cacheKey,
            Func<Task<TItem>> dataRetriever, TimeSpan expiration,
            CancellationToken cancellationToken = default);

        Task RemoveAsync<TItem>(string cacheKey,
            CancellationToken cancellationToken = default);

        Task<long> SetAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null);

        Task<Dictionary<I, T>> HashGetAllAsync<I, T>(string cacheKey);

        Task<bool> HashMSetAsync<I, T>(string cacheKey, Dictionary<I, T> vals, TimeSpan? expiration = null);
    }
}
