using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// A controller action the host's routing serves: one matrix endpoint (spec, Definitions, <i>Endpoint</i>).
    /// </summary>
    /// <param name="Method">Its one HTTP method.</param>
    /// <param name="Route">The route template exactly as routing reports it (<c>RoutePattern.RawText</c>).</param>
    /// <param name="AllowAnonymous">The endpoint carries <see cref="IAllowAnonymous"/> metadata ([AllowAnonymous]).</param>
    /// <param name="Action">The action descriptor.</param>
    /// <param name="Api">ApiExplorer's description of the action: its route, query and body parameters.</param>
    /// <param name="Endpoint">The routing endpoint, which the request recorder reports (FR-015).</param>
    public sealed record DiscoveredEndpoint(string Method, string Route, bool AllowAnonymous, ActionDescriptor Action, ApiDescription? Api, Endpoint Endpoint)
    {
        public string Name => $"{Method} {Route}";
    }

    /// <summary>A routing endpoint that is not a controller action (spec, Definitions, <i>Other endpoints</i>).</summary>
    public sealed record OtherEndpoint(string Name, Endpoint Endpoint)
    {
        public string? Route => (Endpoint as RouteEndpoint)?.RoutePattern.RawText;
    }

    public sealed record Discovery(IReadOnlyList<DiscoveredEndpoint> Actions, IReadOnlyList<OtherEndpoint> Others);

    /// <summary>
    /// Reads the endpoints from the host's routing data (FR-010), not from a hand-kept list, and pairs each controller
    /// action with its ApiExplorer description.
    /// </summary>
    public static class EndpointDiscovery
    {
        public static Discovery Discover(IServiceProvider services) => Discover(
            services.GetRequiredService<EndpointDataSource>().Endpoints,
            services.GetRequiredService<IApiDescriptionGroupCollectionProvider>().ApiDescriptionGroups.Items
                .SelectMany(group => group.Items)
                .ToList());

        public static Discovery Discover(IEnumerable<Endpoint> endpoints, IReadOnlyCollection<ApiDescription> apiDescriptions)
        {
            var actions = new List<DiscoveredEndpoint>();
            var others = new List<OtherEndpoint>();
            foreach (var endpoint in endpoints)
            {
                var action = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (action is null || endpoint is not RouteEndpoint routeEndpoint)
                {
                    others.Add(new OtherEndpoint(endpoint.DisplayName ?? endpoint.ToString() ?? "(unnamed endpoint)", endpoint));
                    continue;
                }

                var methods = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? [];
                if (methods.Count != 1)
                {
                    throw new InvalidOperationException(
                        $"The controller action {endpoint.DisplayName} accepts {(methods.Count == 0 ? "every HTTP method" : string.Join(", ", methods))}. "
                        + "A matrix endpoint has exactly one HTTP method: give the action one [Http*] attribute.");
                }

                actions.Add(new DiscoveredEndpoint(
                    methods[0],
                    routeEndpoint.RoutePattern.RawText ?? string.Empty,
                    endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null,
                    action,
                    apiDescriptions.FirstOrDefault(description => ReferenceEquals(description.ActionDescriptor, action)),
                    endpoint));
            }

            return new Discovery(actions, others);
        }
    }
}
