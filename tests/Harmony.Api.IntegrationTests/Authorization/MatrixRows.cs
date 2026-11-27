using System.Runtime.CompilerServices;
using Harmony.Api.IntegrationTests.Harness;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// Theory data built from the checked-in matrix file, so rows can be listed without Docker (FR-014). Each row is
    /// (method, route) and is named <c>&lt;Rule&gt; &lt;METHOD&gt; &lt;route&gt;</c>. A filter that matches no line yields one
    /// placeholder row, which passes only while no line has that mark, so a theory is never empty and never skipped.
    /// </summary>
    public static class MatrixRows
    {
        private const string PlaceholderMethod = "(none)";

        private static readonly ConditionalWeakTable<ApiTestHost, Discovery> Discoveries = new();

        public static IEnumerable<TheoryDataRow<string, string>> AnonymousRefused() => For(Rule.Anonymous, Mark.Refused);

        public static IEnumerable<TheoryDataRow<string, string>> AnonymousPublic() => For(Rule.Anonymous, Mark.NotApplicable);

        public static IEnumerable<TheoryDataRow<string, string>> AnonymousKnownGaps() => For(Rule.Anonymous, Mark.KnownGap);

        public static IEnumerable<TheoryDataRow<string, string>> NonMemberRefused() => For(Rule.NonMember, Mark.Refused);

        public static IEnumerable<TheoryDataRow<string, string>> NonMemberKnownGaps() => For(Rule.NonMember, Mark.KnownGap);

        public static IEnumerable<TheoryDataRow<string, string>> AdminPlainUser() => Admin(AdminCaller.PlainUser, MatrixFile.Load().Lines);

        public static IEnumerable<TheoryDataRow<string, string>> AdminAllButRequired() => Admin(AdminCaller.AllButRequired, MatrixFile.Load().Lines);

        public static IEnumerable<TheoryDataRow<string, string>> AdminOnlyRequired() => Admin(AdminCaller.OnlyRequired, MatrixFile.Load().Lines);

        /// <summary>One row per administration line, named <c>Admin (&lt;caller&gt;) &lt;METHOD&gt; &lt;route&gt;</c> (FR-110 to FR-112).</summary>
        public static IEnumerable<TheoryDataRow<string, string>> Admin(AdminCaller caller, IEnumerable<MatrixLine> lines)
        {
            var rows = lines
                .Where(line => line.Scope == Scope.Administration)
                .Select(line => new TheoryDataRow<string, string>(line.Method, line.Route) { TestDisplayName = AdminDisplayName(caller, line.Method, line.Route) })
                .ToList();

            return rows.Count > 0
                ? rows
                : [new TheoryDataRow<string, string>(PlaceholderMethod, string.Empty) { TestDisplayName = $"Admin ({CallerName(caller)}) (no administration line)" }];
        }

        public static string AdminDisplayName(AdminCaller caller, string method, string route) => $"Admin ({CallerName(caller)}) {method} {route}";

        public static IEnumerable<TheoryDataRow<string, string>> For(Rule rule, Mark mark, IEnumerable<MatrixLine> lines)
        {
            var rows = lines
                .Where(line => line.MarkFor(rule) == mark && (rule == Rule.Anonymous || line.Scope == Scope.Resource))
                .Select(line => new TheoryDataRow<string, string>(line.Method, line.Route) { TestDisplayName = DisplayName(rule, line.Method, line.Route) })
                .ToList();

            return rows.Count > 0
                ? rows
                : [new TheoryDataRow<string, string>(PlaceholderMethod, string.Empty) { TestDisplayName = $"{RuleName(rule)} (no line: every endpoint is marked)" }];
        }

        public static bool IsPlaceholder(string method) => method == PlaceholderMethod;

        public static string DisplayName(Rule rule, string method, string route) => $"{RuleName(rule)} {method} {route}";

        /// <summary>The host's endpoints, discovered once per host.</summary>
        public static Discovery Discovery(this ApiTestHost host) =>
            Discoveries.GetValue(host, started => EndpointDiscovery.Discover(started.Services));

        private static IEnumerable<TheoryDataRow<string, string>> For(Rule rule, Mark mark) => For(rule, mark, MatrixFile.Load().Lines);

        private static string RuleName(Rule rule) => rule switch
        {
            Rule.Anonymous => "Anonymous",
            Rule.NonMember => "Non-member",
            _ => "Admin",
        };

        private static string CallerName(AdminCaller caller) => caller switch
        {
            AdminCaller.PlainUser => "plain user",
            AdminCaller.AllButRequired => "all but required",
            _ => "only required",
        };
    }
}
