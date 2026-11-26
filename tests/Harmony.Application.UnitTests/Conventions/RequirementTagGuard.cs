using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.v3;

namespace Harmony.Application.UnitTests.Conventions
{
    /// <summary>
    /// Finds test methods that do not carry a <c>[Trait("Requirement", "FR-nnn")]</c> tag.
    /// A test method is any method with an attribute derived from <see cref="FactAttribute"/>
    /// (so theories count too). The tag may be on the method or its class; every tag found must be a requirement ID.
    /// </summary>
    public static class RequirementTagGuard
    {
        public const string TraitName = "Requirement";

        private static readonly Regex RequirementId = new(@"^(FR|NFR)-\d{3}$", RegexOptions.CultureInvariant);

        public static IReadOnlyList<string> FindViolations(IEnumerable<Type> testClasses)
        {
            var violations = new List<string>();

            foreach (var testClass in testClasses)
            {
                var classTags = RequirementTags(testClass);
                var testMethods = testClass
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(method => method.IsDefined(typeof(FactAttribute), inherit: true))
                    .ToList();

                // xUnit merges class and method traits, so class tags are checked even when every method is tagged.
                if (testMethods.Count > 0)
                {
                    AddInvalidTags(violations, testClass.Name, classTags);
                }

                foreach (var method in testMethods)
                {
                    var methodTags = RequirementTags(method);
                    var name = $"{testClass.Name}.{method.Name}";

                    if (methodTags.Count == 0 && classTags.Count == 0)
                    {
                        violations.Add($"{name}: no {TraitName} trait");
                        continue;
                    }

                    AddInvalidTags(violations, name, methodTags);
                }
            }

            return violations;
        }

        private static void AddInvalidTags(List<string> violations, string name, IEnumerable<string> tags)
        {
            foreach (var tag in tags.Where(tag => !RequirementId.IsMatch(tag)))
            {
                violations.Add($"{name}: {TraitName} '{tag}' is not an FR-nnn or NFR-nnn ID");
            }
        }

        private static List<string> RequirementTags(MemberInfo member) =>
            member.GetCustomAttributes(inherit: true)
                .OfType<ITraitAttribute>()
                .SelectMany(attribute => attribute.GetTraits())
                .Where(trait => trait.Key == TraitName)
                .Select(trait => trait.Value)
                .ToList();
    }
}
