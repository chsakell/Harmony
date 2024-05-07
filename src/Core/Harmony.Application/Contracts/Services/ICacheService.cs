using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Services
{
    public interface ICacheService
    {
        Task<TItem?> GetOrCreateAsync<TItem>(string cacheKey,
            CancellationToken cancellationToken = default);

        Task<TItem?> GetOrCreateAsync<TItem>(string cacheKey,
            Func<Task<TItem>> dataRetriever, TimeSpan expiration,
            CancellationToken cancellationToken = default);
    }
}
