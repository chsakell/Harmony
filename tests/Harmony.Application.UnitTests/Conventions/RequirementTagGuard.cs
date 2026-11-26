using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.v3;

namespace Harmony.Application.UnitTests.Conventions
{
    /// <summary>
    /// Finds test methods that do not carry a <c>[Trait("Requirement", "FR-nnn")]</c> tag.
    /// A test method is any method with an attribute derived from <see cref="FactAttribute"/>
    /// (so theories count too). The tag is read from the method or, failing that, from its class.
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
                    .Where(method => method.IsDefined(typeof(FactAttribute), inherit: true));

                foreach (var method in testMethods)
                {
                    var methodTags = RequirementTags(method);
                    var tags = methodTags.Count > 0 ? methodTags : classTags;
                    var name = $"{testClass.Name}.{method.Name}";

                    if (tags.Count == 0)
                    {
                        violations.Add($"{name}: no {TraitName} trait");
                        continue;
                    }

                    foreach (var tag in tags.Where(tag => !RequirementId.IsMatch(tag)))
                    {
                        violations.Add($"{name}: {TraitName} '{tag}' is not an FR-nnn or NFR-nnn ID");
                    }
                }
            }

            return violations;
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
