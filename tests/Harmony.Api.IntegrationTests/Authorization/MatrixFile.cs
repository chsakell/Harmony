using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Harmony.Infrastructure.IntegrationTests.Harness;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>What an endpoint acts on (spec, Definitions, <i>Scope</i>).</summary>
    public enum Scope
    {
        Public,
        Self,
        Resource,
        Administration,
        User,
    }

    /// <summary>A line's mark for one rule: <c>refused</c>, <c>gap R-07</c>/<c>gap R-08</c>, or <c>-</c>. The admin rule has no known gap.</summary>
    public enum Mark
    {
        Refused,
        KnownGap,
        NotApplicable,
    }

    /// <summary>An override that says which resource kind a route value, query parameter or body property names.</summary>
    public sealed record Binding(string Name, string Kind);

    /// <summary>The recorded number of known-gap marks per rule (FR-042).</summary>
    public sealed record Baselines(int AnonymousGaps, int NonMemberGaps);

    /// <summary>One line of the matrix file: one endpoint.</summary>
    /// <param name="Permission">The required permission (<c>permission=</c>) of an administration line; null on other lines.</param>
    public sealed record MatrixLine(
        int LineNumber,
        string Text,
        string Route,
        string Method,
        Scope Scope,
        Mark Anonymous,
        Mark NonMember,
        Mark NonAdmin,
        IReadOnlyList<Binding> Bindings,
        string? FixedBody,
        string? Permission)
    {
        /// <summary><c>METHOD route</c>, as rows and messages name the endpoint.</summary>
        public string Endpoint => $"{Method} {Route}";
    }

    public sealed record MatrixDocument(IReadOnlyList<MatrixLine> Lines, Baselines Baselines)
    {
        public int Count(Rule rule) => Lines.Count(line => line.MarkFor(rule) == Mark.KnownGap);
    }

    /// <summary>The file is not in the contract's shape. The message lists every problem with its line.</summary>
    public sealed class MatrixFileException(string message) : Exception(message);

    /// <summary>
    /// Reads and validates the checked-in matrix file (plan #22, <i>Matrix file contract</i>; FR-012, FR-040, FR-042; and
    /// plan #25, <i>Decisions</i>, Q5: the Non-admin column and the <c>permission=</c> binding, FR-113).
    /// </summary>
    public static class MatrixFile
    {
        public const string Header = "| Route | Method | Scope | Anonymous | Non-member | Non-admin | Bindings |";

        /// <summary>The binding that names an administration line's required permission; it is not a request binding.</summary>
        public const string PermissionBinding = "permission";

        private static readonly IReadOnlySet<string> RegisteredPermissions =
            new HashSet<string>(Harmony.Shared.Constants.Permission.Permissions.GetRegisteredPermissions(), StringComparer.Ordinal);

        public const string AnonymousGap = "gap R-07";

        public const string NonMemberGap = "gap R-08";

        /// <summary>The kinds a binding may name.</summary>
        public static readonly IReadOnlySet<string> Kinds = new HashSet<string>(StringComparer.Ordinal)
        {
            "workspace", "board", "list", "card", "card2", "sprint", "checklist", "checklistitem", "comment", "label",
            "link", "attachment", "issuetype", "caller", "owner", "new",
        };

        private static readonly Regex BaselineLine = new(
            @"^- (?<rule>Anonymous|Non-member) known gaps: (?<count>\d+)\s*$", RegexOptions.CultureInvariant);

        private static readonly Regex MethodText = new("^(GET|POST|PUT|PATCH|DELETE)$", RegexOptions.CultureInvariant);

        private static readonly Lazy<MatrixDocument> Checked = new(() => Parse(File.ReadAllText(FilePath)));

        public static string FilePath => Path.Combine(
            RepositoryRoot.Path, "tests", "Harmony.Api.IntegrationTests", "Authorization", "authorization-matrix.md");

        /// <summary>The checked-in file, parsed once per run.</summary>
        public static MatrixDocument Load() => Checked.Value;

        public static Mark MarkFor(this MatrixLine line, Rule rule) => rule switch
        {
            Rule.Anonymous => line.Anonymous,
            Rule.NonMember => line.NonMember,
            _ => line.NonAdmin,
        };

        public static string GapMark(Rule rule) => rule switch
        {
            Rule.Anonymous => AnonymousGap,
            Rule.NonMember => NonMemberGap,
            _ => throw new ArgumentOutOfRangeException(nameof(rule), rule, "The admin rule has no known gap (FR-113)."),
        };

        public static MatrixDocument Parse(string text)
        {
            var errors = new List<string>();
            var rawLines = text.ReplaceLineEndings("\n").Split('\n');

            var baselines = ParseBaselines(rawLines, errors);
            var lines = ParseEndpoints(rawLines, errors);

            if (errors.Count > 0)
            {
                throw new MatrixFileException(
                    $"{FilePathForMessages} is not valid:{Environment.NewLine}- " + string.Join(Environment.NewLine + "- ", errors));
            }

            return new MatrixDocument(lines, baselines!);
        }

        private static string FilePathForMessages => "tests/Harmony.Api.IntegrationTests/Authorization/authorization-matrix.md";

        private static Baselines? ParseBaselines(string[] rawLines, List<string> errors)
        {
            int? anonymous = null;
            int? nonMember = null;
            foreach (var raw in rawLines)
            {
                var match = BaselineLine.Match(raw.Trim());
                if (!match.Success)
                {
                    continue;
                }

                var count = int.Parse(match.Groups["count"].Value, CultureInfo.InvariantCulture);
                if (match.Groups["rule"].Value == "Anonymous")
                {
                    anonymous = count;
                }
                else
                {
                    nonMember = count;
                }
            }

            if (anonymous is null)
            {
                errors.Add("the Baselines section has no line '- Anonymous known gaps: <n>'");
            }

            if (nonMember is null)
            {
                errors.Add("the Baselines section has no line '- Non-member known gaps: <n>'");
            }

            return anonymous is null || nonMember is null ? null : new Baselines(anonymous.Value, nonMember.Value);
        }

        private static List<MatrixLine> ParseEndpoints(string[] rawLines, List<string> errors)
        {
            var lines = new List<MatrixLine>();
            var headerIndex = Array.FindIndex(rawLines, raw => raw.Trim().StartsWith("| Route ", StringComparison.Ordinal));
            if (headerIndex < 0 || rawLines[headerIndex].Trim() != Header)
            {
                errors.Add($"the Endpoints table header must be exactly '{Header}'"
                    + (headerIndex < 0 ? string.Empty : $" (line {headerIndex + 1}: {rawLines[headerIndex].Trim()})"));
                return lines;
            }

            var seen = new HashSet<(string Route, string Method)>();
            MatrixLine? previous = null;
            for (var index = headerIndex + 2; index < rawLines.Length; index++)
            {
                var text = rawLines[index].Trim();
                if (!text.StartsWith('|'))
                {
                    break;
                }

                var line = ParseLine(index + 1, text, errors);
                if (line is null)
                {
                    continue;
                }

                if (!seen.Add((line.Route, line.Method)))
                {
                    errors.Add(At(line.LineNumber, text, "is a duplicate of an earlier line"));
                }
                else if (previous is not null && Compare(previous, line) > 0)
                {
                    errors.Add(At(line.LineNumber, text,
                        "is out of order: lines are sorted by Route, then Method (ordinal); it belongs before line "
                        + previous.LineNumber.ToString(CultureInfo.InvariantCulture)));
                }

                previous = line;
                lines.Add(line);
            }

            return lines;
        }

        private static int Compare(MatrixLine left, MatrixLine right)
        {
            var route = StringComparer.Ordinal.Compare(left.Route, right.Route);
            return route != 0 ? route : StringComparer.Ordinal.Compare(left.Method, right.Method);
        }

        private static MatrixLine? ParseLine(int lineNumber, string text, List<string> errors)
        {
            var cells = text.Trim('|').Split('|').Select(cell => cell.Trim()).ToArray();
            if (cells.Length != 7)
            {
                errors.Add(At(lineNumber, text,
                    $"has {cells.Length} cells; a line has 7 cells (Route, Method, Scope, Anonymous, Non-member, Non-admin, Bindings)"));
                return null;
            }

            var problems = new List<string>();
            var route = cells[0];
            var method = cells[1];
            if (route.Length == 0)
            {
                problems.Add("has no route");
            }

            if (!MethodText.IsMatch(method))
            {
                problems.Add($"has an unknown method '{method}' (GET, POST, PUT, PATCH or DELETE, in capitals)");
            }

            Scope? scope = TryParseScope(cells[2], out var parsedScope) ? parsedScope : null;
            if (scope is null)
            {
                problems.Add($"has an unknown scope '{cells[2]}' (public, self, resource, administration or user)");
            }

            var anonymous = ParseMark(cells[3], Rule.Anonymous, problems);
            var nonMember = ParseMark(cells[4], Rule.NonMember, problems);

            if (scope is not null && anonymous is not null && (scope == Scope.Public) != (anonymous == Mark.NotApplicable))
            {
                problems.Add(scope == Scope.Public
                    ? $"has the Anonymous mark '{cells[3]}', but a public endpoint's Anonymous mark is '-'"
                    : $"has the Anonymous mark '-', which only a public endpoint has; use 'refused' or '{AnonymousGap}'");
            }

            if (scope is not null && nonMember is not null && (scope == Scope.Resource) == (nonMember == Mark.NotApplicable))
            {
                problems.Add(scope == Scope.Resource
                    ? $"has the Non-member mark '-', but a resource endpoint's Non-member mark is 'refused' or '{NonMemberGap}'"
                    : $"has the Non-member mark '{cells[4]}', but only a resource endpoint has one; use '-'");
            }

            if (scope == Scope.Administration && anonymous == Mark.KnownGap)
            {
                problems.Add($"has the Anonymous mark '{cells[3]}', but an administration endpoint ships closed and may have no known gap (FR-113)");
            }

            var nonAdmin = ParseNonAdminMark(cells[5], scope, problems);
            var (bindings, fixedBody, permission) = ParseBindings(cells[6], problems);

            if (scope == Scope.Administration && permission is null)
            {
                problems.Add($"names no required permission; an administration endpoint names one with {PermissionBinding}=<value>, "
                    + "for example permission=Permissions.Roles.View");
            }

            if (scope is not null && scope != Scope.Administration && permission is not null)
            {
                problems.Add($"names the permission '{permission}', but only an administration endpoint names a permission");
            }

            foreach (var problem in problems)
            {
                errors.Add(At(lineNumber, text, problem));
            }

            return problems.Count > 0
                ? null
                : new MatrixLine(lineNumber, text, route, method, scope!.Value, anonymous!.Value, nonMember!.Value, nonAdmin!.Value, bindings,
                    fixedBody, permission);
        }

        /// <summary>
        /// The Non-admin mark is <c>refused</c> on an administration line and <c>-</c> on every other. The admin rule has
        /// no known gap: an administration endpoint ships closed (FR-113).
        /// </summary>
        private static Mark? ParseNonAdminMark(string text, Scope? scope, List<string> problems)
        {
            Mark? mark = text switch
            {
                "refused" => Mark.Refused,
                "-" => Mark.NotApplicable,
                _ => null,
            };

            if (mark is null)
            {
                problems.Add(text.StartsWith("gap ", StringComparison.Ordinal)
                    ? $"has the Non-admin mark '{text}', but the admin rule has no known gap: an administration endpoint ships refused (FR-113)"
                    : $"has an unknown mark '{text}' in Non-admin (refused or -)");
                return null;
            }

            if (scope is not null && (scope == Scope.Administration) != (mark == Mark.Refused))
            {
                problems.Add(scope == Scope.Administration
                    ? "has the Non-admin mark '-', but an administration endpoint's Non-admin mark is 'refused'"
                    : $"has the Non-admin mark '{text}', but only an administration endpoint has one; use '-'");
            }

            return mark;
        }

        /// <summary>Scopes are written exactly in lower case; numbers and other spellings are unknown.</summary>
        private static bool TryParseScope(string text, out Scope scope)
        {
            foreach (var candidate in Enum.GetValues<Scope>())
            {
                if (text == candidate.ToString().ToLowerInvariant())
                {
                    scope = candidate;
                    return true;
                }
            }

            scope = default;
            return false;
        }

        private static Mark? ParseMark(string text, Rule rule, List<string> problems)
        {
            var column = rule == Rule.Anonymous ? "Anonymous" : "Non-member";
            switch (text)
            {
                case "refused":
                    return Mark.Refused;
                case "-":
                    return Mark.NotApplicable;
                case var gap when gap.StartsWith("gap ", StringComparison.Ordinal):
                    if (gap == GapMark(rule))
                    {
                        return Mark.KnownGap;
                    }

                    problems.Add($"has the {column} mark '{gap}', but a {column} known gap must name {GapMark(rule)[4..]} ('{GapMark(rule)}')");
                    return null;
                default:
                    problems.Add($"has an unknown mark '{text}' in {column} (refused, {GapMark(rule)} or -)");
                    return null;
            }
        }

        /// <returns>The permission as written, even when it is not a registered one, so that only one problem names it.</returns>
        private static (IReadOnlyList<Binding> Bindings, string? FixedBody, string? Permission) ParseBindings(string text, List<string> problems)
        {
            var bindings = new List<Binding>();
            string? fixedBody = null;
            string? permission = null;
            var rest = text.Trim();
            while (rest.Length > 0)
            {
                if (rest.StartsWith("body=", StringComparison.Ordinal))
                {
                    fixedBody = rest["body=".Length..].Trim();
                    try
                    {
                        using var _ = JsonDocument.Parse(fixedBody);
                    }
                    catch (JsonException exception)
                    {
                        problems.Add($"has a body= that is not valid JSON ({exception.Message}); body= must be the last binding");
                        fixedBody = null;
                    }

                    break;
                }

                var separator = rest.IndexOf(';', StringComparison.Ordinal);
                var item = (separator < 0 ? rest : rest[..separator]).Trim();
                rest = separator < 0 ? string.Empty : rest[(separator + 1)..].Trim();

                var equals = item.IndexOf('=', StringComparison.Ordinal);
                if (equals <= 0 || equals == item.Length - 1)
                {
                    problems.Add($"has the binding '{item}', which is not name=kind");
                    continue;
                }

                var name = item[..equals].Trim();
                var kind = item[(equals + 1)..].Trim();
                if (name == PermissionBinding)
                {
                    if (permission is not null)
                    {
                        problems.Add($"names {PermissionBinding}= twice");
                    }
                    else if (!RegisteredPermissions.Contains(kind))
                    {
                        problems.Add($"names an unknown permission '{kind}' (a value of Permissions.GetRegisteredPermissions())");
                    }

                    permission ??= kind;
                    continue;
                }

                if (!Kinds.Contains(kind))
                {
                    problems.Add($"has an unknown kind '{kind}' in '{item}' ({string.Join(", ", Kinds)})");
                    continue;
                }

                if (bindings.Any(binding => string.Equals(binding.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    problems.Add($"binds '{name}' twice");
                    continue;
                }

                bindings.Add(new Binding(name, kind));
            }

            return (bindings, fixedBody, permission);
        }

        private static string At(int lineNumber, string text, string problem) =>
            $"line {lineNumber.ToString(CultureInfo.InvariantCulture)} {problem}: {text}";
    }
}
