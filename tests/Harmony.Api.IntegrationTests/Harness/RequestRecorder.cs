using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Harmony.Api.IntegrationTests.Harness
{
    /// <summary>
    /// Test-only diagnostic for FR-015: the outermost middleware, which records, for each request that carries
    /// <see cref="HeaderName"/>, the endpoint routing selected. When the exception handler has re-executed the request
    /// to <c>/Error</c>, the endpoint that threw is read from <see cref="IExceptionHandlerFeature.Endpoint"/>.
    /// </summary>
    public sealed class RequestRecorder : IStartupFilter
    {
        public const string HeaderName = "X-Matrix-Request";

        private readonly ConcurrentDictionary<string, Endpoint?> selected = new(StringComparer.Ordinal);

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                var id = context.Request.Headers[HeaderName].ToString();
                try
                {
                    await nextMiddleware(context);
                }
                finally
                {
                    if (id.Length > 0)
                    {
                        selected[id] = context.Features.Get<IExceptionHandlerFeature>()?.Endpoint ?? context.GetEndpoint();
                    }
                }
            });
            next(app);
        };

        /// <summary>The endpoint routing selected for the request, or null when it selected none or never saw it.</summary>
        public Endpoint? SelectedFor(string requestId) => selected.TryGetValue(requestId, out var endpoint) ? endpoint : null;

        /// <summary>True once a request with this ID has passed through the pipeline.</summary>
        public bool Saw(string requestId) => selected.ContainsKey(requestId);
    }
}
