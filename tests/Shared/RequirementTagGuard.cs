using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.v3;

namespace Harmony.Tests.Shared
{
    /// <summary>
    /// Finds test methods that are not traceable to a requirement or a bug. A test method is any method with an
    /// attribute derived from <see cref="FactAttribute"/> (so theories count too). It needs a
    /// <c>[Trait("Requirement", "FR-nnn")]</c> tag or, for a Track S bug fix, a <c>[Trait("Bug", "&lt;issue&gt;")]</c>
    /// tag with the GitHub issue number. Tags may be on the method or its class; every tag found must be well formed.
    /// Compiled into every test project through <c>tests/Directory.Build.props</c>.
    /// </summary>
    public static class RequirementTagGuard
    {
        public const string TraitName = "Requirement";
        public const string BugTraitName = "Bug";

        private static readonly Regex RequirementId = new(@"^(FR|NFR)-\d{3}$", RegexOptions.CultureInvariant);
        private static readonly Regex IssueNumber = new(@"^[1-9]\d*$", RegexOptions.CultureInvariant);

        public static IReadOnlyList<string> FindViolations(IEnumerable<Type> testClasses)
        {
            var violations = new List<string>();

            foreach (var testClass in testClasses)
            {
                var classTags = Tags(testClass, TraitName);
                var classBugTags = Tags(testClass, BugTraitName);
                var testMethods = testClass
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(method => method.IsDefined(typeof(FactAttribute), inherit: true))
                    .ToList();

                // xUnit merges class and method traits, so class tags are checked even when every method is tagged.
                if (testMethods.Count > 0)
                {
                    AddInvalidTags(violations, testClass.Name, classTags, classBugTags);
                }

                foreach (var method in testMethods)
                {
                    var methodTags = Tags(method, TraitName);
                    var methodBugTags = Tags(method, BugTraitName);
                    var name = $"{testClass.Name}.{method.Name}";

                    if (methodTags.Count == 0 && classTags.Count == 0
                        && methodBugTags.Count == 0 && classBugTags.Count == 0)
                    {
                        violations.Add($"{name}: no {TraitName} or {BugTraitName} trait");
                        continue;
                    }

                    AddInvalidTags(violations, name, methodTags, methodBugTags);
                }
            }

            return violations;
        }

        private static void AddInvalidTags(
            List<string> violations, string name, IEnumerable<string> tags, IEnumerable<string> bugTags)
        {
            foreach (var tag in tags.Where(tag => !RequirementId.IsMatch(tag)))
            {
                violations.Add($"{name}: {TraitName} '{tag}' is not an FR-nnn or NFR-nnn ID");
            }

            foreach (var tag in bugTags.Where(tag => !IssueNumber.IsMatch(tag)))
            {
                violations.Add($"{name}: {BugTraitName} '{tag}' is not a GitHub issue number");
            }
        }

        private static List<string> Tags(MemberInfo member, string traitName) =>
            member.GetCustomAttributes(inherit: true)
                .OfType<ITraitAttribute>()
                .SelectMany(attribute => attribute.GetTraits())
                .Where(trait => trait.Key == traitName)
                .Select(trait => trait.Value)
                .ToList();
    }
}
