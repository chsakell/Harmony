using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Harmony.Api.IntegrationTests.Harness;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// Builds matrix requests from an endpoint's route template and ApiExplorer description (spec, Definitions,
    /// <i>Request values</i>; FR-020, FR-031).
    /// </summary>
    public static partial class RequestBuilder
    {
        /// <summary>The placeholder for text: 6 characters, enough for the minimum lengths of 5 and 6.</summary>
        public const string TextPlaceholder = "Matrix";

        private const int MaxDepth = 4;

        /// <summary>The default name map (spec, Definitions, <i>Name map</i>), matched without regard to case.</summary>
        private static readonly Dictionary<string, string> NameMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["BoardId"] = "board",
            ["ListId"] = "list",
            ["BoardListId"] = "list",
            ["CardId"] = "card",
            ["SourceCardId"] = "card",
            ["TargetCardId"] = "card2",
            ["WorkspaceId"] = "workspace",
            ["SprintId"] = "sprint",
            ["MoveToSprintId"] = "sprint",
            ["CheckListId"] = "checklist",
            ["ListItemId"] = "checklistitem",
            ["CommentId"] = "comment",
            ["LabelId"] = "label",
            ["LinkId"] = "link",
            ["AttachmentId"] = "attachment",
            ["IssueTypeId"] = "issuetype",
            ["UserId"] = "caller",
        };

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
                request.Content = Json("{}");
            }

            return request;
        }

        /// <summary>
        /// The non-member rule's request, or the owner's (FR-031): every route value, query parameter and body property
        /// that the line's bindings or the name map assign a kind gets the fixture's ID of that kind (the caller's own ID
        /// for <c>caller</c>). Nested objects are filled too, and a list of IDs gets one element. Other body properties get
        /// their type's default, and text <see cref="TextPlaceholder"/>. A line's <c>body=</c> is sent instead, with its ID
        /// properties replaced. An ID-named value that maps to no kind fails, naming it.
        /// </summary>
        public static HttpRequestMessage ForCaller(DiscoveredEndpoint endpoint, MatrixLine line, Fixture fixture, TestUser caller)
        {
            var ids = new IdResolver(endpoint, line, fixture, caller);
            var parameters = Parameters(endpoint);

            var path = FillRoute(endpoint.Route, parameters, name => ids.Resolve(name, "route value"));
            var query = new List<string>();
            foreach (var parameter in parameters.Where(parameter => parameter.Source == BindingSource.Query))
            {
                if (IsIdName(parameter.Name) || IsIdsName(parameter.Name))
                {
                    query.Add($"{Uri.EscapeDataString(parameter.Name)}={Uri.EscapeDataString(ids.Resolve(Singular(parameter.Name), "query parameter", parameter.Name))}");
                }
                else if (parameter.IsRequired || (parameter.ModelMetadata?.IsRequired == true && parameter.Type is { IsValueType: false }))
                {
                    // A required parameter left out makes the request a 400 for the owner too (inconclusive), so it gets
                    // a placeholder of its type, as a route value does. MVC treats a non-nullable string as required
                    // (ModelMetadata.IsRequired); non-nullable value types report required too but keep their default.
                    query.Add($"{Uri.EscapeDataString(parameter.Name)}={Placeholder(parameter.Type)}");
                }
            }

            var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), new Uri(WithQuery(path, query), UriKind.Relative));
            var body = parameters.FirstOrDefault(parameter => parameter.Source == BindingSource.Body);
            if (line.FixedBody is not null)
            {
                var node = JsonNode.Parse(line.FixedBody);
                ReplaceIds(node, path: string.Empty, ids);
                request.Content = Json(node?.ToJsonString() ?? "null");
            }
            else if (body is not null)
            {
                request.Content = Json(BuildValue(body.Type, path: string.Empty, ids, depth: 0)?.ToJsonString() ?? "null");
            }

            return request;
        }

        /// <summary>
        /// A name that is <c>id</c> (any case) or ends in <c>Id</c> (exact case) names an ID: <c>id</c>, <c>boardId</c>,
        /// <c>BoardId</c>, but not <c>Valid</c>, <c>Paid</c> or <c>Guid</c>.
        /// </summary>
        public static bool IsIdName(string name) =>
            name.Equals("id", StringComparison.OrdinalIgnoreCase) || name.EndsWith("Id", StringComparison.Ordinal);

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

        private static string Placeholder(Type? type)
        {
            var underlying = Nullable.GetUnderlyingType(type ?? typeof(string)) ?? type;
            return underlying == typeof(bool) ? "false"
                : underlying == typeof(int) || underlying == typeof(long) || underlying == typeof(short) || underlying == typeof(byte) ? "1"
                : TextPlaceholder;
        }

        /// <summary>The plural of <see cref="IsIdName"/>: <c>ids</c> (any case) or a name ending in <c>Ids</c>, not <c>Kids</c>.</summary>
        private static bool IsIdsName(string name) =>
            name.Equals("ids", StringComparison.OrdinalIgnoreCase) || name.EndsWith("Ids", StringComparison.Ordinal);

        /// <summary><c>ListIds</c> maps through <c>ListId</c>.</summary>
        private static string Singular(string name) => IsIdsName(name) ? name[..^1] : name;

        private static string Join(string path, string name) => path.Length == 0 ? name : $"{path}.{name}";

        private static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");

        private static JsonNode? BuildValue(Type type, string path, IdResolver ids, int depth)
        {
            var name = path.Length == 0 ? string.Empty : path[(path.LastIndexOf('.') + 1)..];
            var underlying = Nullable.GetUnderlyingType(type) ?? type;

            if (name.Length > 0 && IsIdName(name))
            {
                if (underlying != typeof(Guid) && underlying != typeof(string))
                {
                    // A fixture ID is a GUID, so it cannot go in this property; a default would hide the gap (FR-031).
                    throw new InvalidOperationException(
                        $"{ids.EndpointName}: the body property '{path}' names an ID, but its type {underlying.Name} cannot hold a fixture ID. "
                        + "Give the line a 'body=' with the value to send.");
                }

                return JsonValue.Create(ids.Resolve(name, "body property", path));
            }

            if (underlying == typeof(string))
            {
                return JsonValue.Create(TextPlaceholder);
            }

            if (underlying == typeof(byte[]))
            {
                return null;
            }

            if (underlying != typeof(string) && typeof(IEnumerable).IsAssignableFrom(underlying))
            {
                if (IsDictionary(underlying))
                {
                    return new JsonObject();
                }

                var array = new JsonArray();
                if (name.Length > 0 && IsIdsName(name))
                {
                    array.Add(JsonValue.Create(ids.Resolve(Singular(name), "body property", path)));
                }

                return array;
            }

            if (underlying == typeof(bool))
            {
                return Nullable.GetUnderlyingType(type) is null ? JsonValue.Create(false) : null;
            }

            if (underlying.IsEnum || underlying.IsPrimitive || underlying == typeof(decimal))
            {
                return Nullable.GetUnderlyingType(type) is null ? JsonValue.Create(0) : null;
            }

            if (underlying.IsValueType)
            {
                // Guid (not ID-named), DateTime, TimeSpan and other structs: their default, or null when nullable.
                return Nullable.GetUnderlyingType(type) is null
                    ? JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(Activator.CreateInstance(underlying)))
                    : null;
            }

            if (depth >= MaxDepth)
            {
                return null;
            }

            var json = new JsonObject();
            foreach (var property in underlying.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetIndexParameters().Length > 0 || property.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
                {
                    continue;
                }

                json[property.Name] = BuildValue(property.PropertyType, Join(path, property.Name), ids, depth + 1);
            }

            return json;
        }

        private static bool IsDictionary(Type type) =>
            typeof(IDictionary).IsAssignableFrom(type)
            || type.GetInterfaces().Append(type).Any(candidate => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IDictionary<,>));

        /// <summary>Replaces the ID-named properties of a fixed body, in nested objects and arrays too.</summary>
        private static void ReplaceIds(JsonNode? node, string path, IdResolver ids)
        {
            switch (node)
            {
                case JsonObject json:
                    foreach (var (name, value) in json.ToList())
                    {
                        var propertyPath = Join(path, name);
                        if (IsIdName(name) && value is JsonValue)
                        {
                            json[name] = JsonValue.Create(ids.Resolve(name, "body property", propertyPath));
                        }
                        else if (IsIdsName(name) && value is JsonArray array)
                        {
                            json[name] = new JsonArray(array.Select(_ => (JsonNode?)JsonValue.Create(ids.Resolve(Singular(name), "body property", propertyPath))).ToArray());
                        }
                        else
                        {
                            ReplaceIds(value, propertyPath, ids);
                        }
                    }

                    break;
                case JsonArray array:
                    foreach (var item in array)
                    {
                        ReplaceIds(item, path, ids);
                    }

                    break;
            }
        }

        [GeneratedRegex(@"\{\*{0,2}(?<name>[^}:=?]+)(?<constraints>[^}]*)\}", RegexOptions.CultureInvariant)]
        private static partial Regex RouteParameter();

        /// <summary>Maps a value's name or path to its kind through the line's bindings, then the name map.</summary>
        private sealed class IdResolver(DiscoveredEndpoint endpoint, MatrixLine line, Fixture fixture, TestUser caller)
        {
            public string EndpointName => endpoint.Name;

            /// <param name="name">The value's own name, used for the name map (singular for a list of IDs).</param>
            /// <param name="where">What the value is, for the failure message.</param>
            /// <param name="path">The value's full name for bindings (a body property path); defaults to <paramref name="name"/>.</param>
            public string Resolve(string name, string where, string? path = null)
            {
                path ??= name;
                var binding = line.Bindings.FirstOrDefault(candidate => string.Equals(candidate.Name, path, StringComparison.OrdinalIgnoreCase))
                    ?? (path != name ? line.Bindings.FirstOrDefault(candidate => string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase)) : null);
                var kind = binding?.Kind ?? (NameMap.TryGetValue(name, out var mapped) ? mapped : null);

                return kind switch
                {
                    null => throw new InvalidOperationException(
                        $"{endpoint.Name}: the {where} '{path}' names an ID, but neither the name map nor a binding on line {line.LineNumber} "
                        + $"gives its kind. Add '{path}=<kind>' to the line's Bindings ({string.Join(", ", MatrixFile.Kinds)})."),
                    "workspace" => fixture.WorkspaceId.ToString(),
                    "board" => fixture.BoardId.ToString(),
                    "list" => fixture.ListId.ToString(),
                    "card" => fixture.CardId.ToString(),
                    "card2" => fixture.Card2Id.ToString(),
                    "sprint" => fixture.SprintId.ToString(),
                    "checklist" => fixture.ChecklistId.ToString(),
                    "checklistitem" => fixture.ChecklistItemId.ToString(),
                    "comment" => fixture.CommentId.ToString(),
                    "label" => fixture.LabelId.ToString(),
                    "link" => fixture.LinkId.ToString(),
                    "attachment" => fixture.AttachmentId.ToString(),
                    "issuetype" => fixture.IssueTypeId.ToString(),
                    "caller" => caller.Id,
                    "owner" => fixture.OwnerId,
                    "new" => Guid.NewGuid().ToString(),
                    _ => throw new InvalidOperationException($"Unknown kind '{kind}' on line {line.LineNumber}."),
                };
            }
        }
    }
}
