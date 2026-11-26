using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// Builds matrix requests from an endpoint's route template and ApiExplorer description (spec, Definitions,
    /// <i>Request values</i>).
    /// </summary>
    public static partial class RequestBuilder
    {
        /// <summary>The placeholder for route values that are neither IDs nor numbers (6 characters).</summary>
        public const string TextPlaceholder = "Matrix";

        /// <summary>
        /// The anonymous rule's request (FR-020): IDs in the route and query get new random GUIDs, other route values a
        /// placeholder of their type, other query parameters are left out, and an action with a body gets <c>{}</c>.
        /// </summary>
        public static HttpRequestMessage Anonymous(DiscoveredEndpoint endpoint)
        {
            var parameters = Parameters(endpoint);
            var path = FillRoute(endpoint.Route, parameters, _ => Guid.NewGuid().ToString());
            var query = parameters
                .Where(parameter => parameter.Source == BindingSource.Query && IsIdName(parameter.Name))
                .Select(parameter => $"{Uri.EscapeDataString(parameter.Name)}={Guid.NewGuid()}");

            var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), new Uri(WithQuery(path, query), UriKind.Relative));
            if (parameters.Any(parameter => parameter.Source == BindingSource.Body))
            {
                request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
            }

            return request;
        }

        /// <summary>A name that ends in <c>Id</c> (any case) names an ID.</summary>
        public static bool IsIdName(string name) => name.EndsWith("Id", StringComparison.OrdinalIgnoreCase);

        internal static IReadOnlyList<ApiParameterDescription> Parameters(DiscoveredEndpoint endpoint) =>
            endpoint.Api?.ParameterDescriptions.ToList()
            ?? throw new InvalidOperationException($"ApiExplorer has no description of {endpoint.Name}, so its parameters are unknown.");

        /// <summary>
        /// Replaces each route parameter of <paramref name="template"/>: GUIDs and ID names get <paramref name="id"/>,
        /// numbers get <c>1</c>, other values <see cref="TextPlaceholder"/>.
        /// </summary>
        internal static string FillRoute(string template, IReadOnlyList<ApiParameterDescription> parameters, Func<string, string> id) =>
            RouteParameter().Replace(template, match =>
            {
                var name = match.Groups["name"].Value;
                var constraints = match.Groups["constraints"].Value;
                var type = parameters.FirstOrDefault(parameter =>
                    parameter.Source == BindingSource.Path && string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase))?.Type;
                type = Nullable.GetUnderlyingType(type ?? typeof(string)) ?? type;

                if (constraints.Contains(":guid", StringComparison.Ordinal) || type == typeof(Guid) || IsIdName(name))
                {
                    return id(name);
                }

                if (constraints.Contains(":int", StringComparison.Ordinal) || constraints.Contains(":long", StringComparison.Ordinal)
                    || type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
                {
                    return "1";
                }

                return TextPlaceholder;
            });

        internal static string WithQuery(string path, IEnumerable<string> query)
        {
            var items = query.ToList();
            return items.Count == 0 ? path : $"{path}?{string.Join('&', items)}";
        }

        [GeneratedRegex(@"\{\*{0,2}(?<name>[^}:=?]+)(?<constraints>[^}]*)\}", RegexOptions.CultureInvariant)]
        private static partial Regex RouteParameter();
    }
}
