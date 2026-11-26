using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Harmony.Infrastructure.UnitTests.TestSupport
{
    /// <summary>
    /// Wraps in-memory entities in an <see cref="IQueryable{T}"/> that EF Core's async operators
    /// (<c>ToListAsync</c>, <c>FirstOrDefaultAsync</c>, ...) accept, so a repository's <c>Entities</c>
    /// can be stubbed with plain lists. Queries run as LINQ to Objects: they prove the query logic,
    /// not how a database provider translates it.
    /// </summary>
    public static class AsyncQueryable
    {
        public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source) =>
            new AsyncEnumerableQuery<T>(source);
    }

    internal sealed class AsyncEnumerableQuery<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public AsyncEnumerableQuery(IEnumerable<T> source)
            : base(source)
        {
        }

        public AsyncEnumerableQuery(Expression expression)
            : base(expression)
        {
        }

        IQueryProvider IQueryable.Provider => new AsyncQueryProvider(this);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new AsyncEnumerator(this.AsEnumerable().GetEnumerator());

        private sealed class AsyncEnumerator(IEnumerator<T> inner) : IAsyncEnumerator<T>
        {
            public T Current => inner.Current;

            public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(inner.MoveNext());

            public ValueTask DisposeAsync()
            {
                inner.Dispose();
                return ValueTask.CompletedTask;
            }
        }
    }

    internal sealed class AsyncQueryProvider(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetInterfaces()
                .Append(expression.Type)
                .First(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .GetGenericArguments()[0];

            return (IQueryable)Activator.CreateInstance(
                typeof(AsyncEnumerableQuery<>).MakeGenericType(elementType), expression)!;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            new AsyncEnumerableQuery<TElement>(expression);

        public object? Execute(Expression expression) => inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);

        // EF Core asks for Task<T> (for example FirstOrDefaultAsync); run the query synchronously and wrap the result.
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];

            var result = typeof(IQueryProvider)
                .GetMethods()
                .Single(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
                .MakeGenericMethod(resultType)
                .Invoke(inner, [expression]);

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, [result])!;
        }
    }
}
