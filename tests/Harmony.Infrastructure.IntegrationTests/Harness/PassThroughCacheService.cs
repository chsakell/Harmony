using Harmony.Application.Contracts.Services;
using Harmony.Domain.Contracts;

// ICacheService is declared without nullable annotations (its generic keys are unconstrained), so this
// implementation uses the same context; with annotations on, Dictionary<I, T> fails CS8714.
#nullable disable

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// A cache that never holds anything: GetOrCreate calls the retriever, reads find nothing and writes report
    /// success. Every read of the code under test therefore reaches the database, which is what these tests prove.
    /// </summary>
    public sealed class PassThroughCacheService : ICacheService
    {
        public Task<TItem> GetAsync<TItem>(string cacheKey, CancellationToken cancellationToken = default) =>
            Task.FromResult(default(TItem));

        public async Task<TItem> GetOrCreateAsync<TItem>(string cacheKey, Func<Task<TItem>> dataRetriever,
            TimeSpan expiration, CancellationToken cancellationToken = default) => await dataRetriever();

        public Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<Dictionary<string, string>> HashGetAllAsync(string cacheKey) => Task.FromResult(new Dictionary<string, string>());

        public Task<Dictionary<I, T>> HashGetAllAsync<I, T>(string cacheKey) => Task.FromResult(new Dictionary<I, T>());

        public Task<Dictionary<I, T>> HashMGetFields<I, T>(string cacheKey, List<string> fields) => Task.FromResult(new Dictionary<I, T>());

        public Task<T> HashGetAllAsync<T>(string cacheKey, Func<Dictionary<string, string>, T> converter) =>
            Task.FromResult(default(T));

        public async Task<T> HashGetAllOrCreateAsync<T>(string cacheKey, Func<Dictionary<string, string>, T> converter,
            Func<Task<T>> dataRetriever, TimeSpan? expiration = null)
            where T : IHashable => await dataRetriever();

        public Task<T> HashGetAsync<T>(string cacheKey, string field) => Task.FromResult(default(T));

        public Task<bool> HashMSetAsync<I, T>(string cacheKey, Dictionary<I, T> vals, TimeSpan? expiration = null) => Task.FromResult(true);

        public Task<bool> HashMSetAsync(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null) => Task.FromResult(true);

        public Task<bool> HashHSetAsync(string cacheKey, string field, string value) => Task.FromResult(true);

        public Task<long> HashDeleleFields(string cacheKey, IList<string> fields = null) => Task.FromResult(0L);

        public Task<long> HashDeleleField(string cacheKey, string field) => Task.FromResult(0L);
    }
}
