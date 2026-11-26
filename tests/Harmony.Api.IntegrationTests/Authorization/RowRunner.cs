using Harmony.Api.IntegrationTests.Harness;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// Sends a matrix request and reports its status, body and whether routing selected the endpoint under test
    /// (FR-015), read from the <see cref="RequestRecorder"/>.
    /// </summary>
    public static class RowRunner
    {
        public const string NoEndpoint = "no endpoint (no route matched, or the index.html fallback, which clears the endpoint)";

        public static async Task<RowResponse> SendAsync(ApiTestHost api, DiscoveredEndpoint endpoint, HttpRequestMessage request, string? token,
            CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid().ToString("N");
            request.Headers.Remove(RequestRecorder.HeaderName);
            request.Headers.Add(RequestRecorder.HeaderName, requestId);

            using var client = api.CreateClient(token);
            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            var selected = api.Recorder.SelectedFor(requestId);
            var reached = selected is not null
                && (ReferenceEquals(selected, endpoint.Endpoint)
                    || ReferenceEquals(selected.Metadata.GetMetadata<ActionDescriptor>(), endpoint.Action));

            return new RowResponse((int)response.StatusCode, reached, selected?.DisplayName ?? NoEndpoint, body);
        }
    }
}
