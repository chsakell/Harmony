using Harmony.Api.IntegrationTests.Harness;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>The non-member request on one fixture and the owner's same request on another (FR-030, FR-033).</summary>
    public sealed record NonMemberRun(RowResponse NonMember, RowResponse Owner);

    /// <summary>
    /// Sends a matrix request and reports its status, body and whether routing selected the endpoint under test
    /// (FR-015), read from the <see cref="RequestRecorder"/>.
    /// </summary>
    public static class RowRunner
    {
        public const string NoEndpoint = "no endpoint (no route matched, or the index.html fallback, which clears the endpoint)";

        /// <summary>
        /// Sends the non-member's request with <paramref name="nonMemberFixture"/>'s IDs, then the owner's same request with
        /// <paramref name="ownerFixture"/>'s, so neither request can change what the other sees (FR-032, FR-033).
        /// </summary>
        public static async Task<NonMemberRun> RunNonMemberAsync(ApiTestHost api, DiscoveredEndpoint endpoint, MatrixLine line, Fixture nonMemberFixture,
            Fixture ownerFixture, CancellationToken cancellationToken)
        {
            using var nonMemberRequest = RequestBuilder.ForCaller(endpoint, line, nonMemberFixture, api.NonMember);
            var nonMember = await SendAsync(api, endpoint, nonMemberRequest, api.Tokens.For(api.NonMember), cancellationToken);

            using var ownerRequest = RequestBuilder.ForCaller(endpoint, line, ownerFixture, api.Owner);
            var owner = await SendAsync(api, endpoint, ownerRequest, api.Tokens.For(api.Owner), cancellationToken);

            return new NonMemberRun(nonMember, owner);
        }

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
