using Harmony.Tests.Shared;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Conventions
{
    /// <summary>
    /// The guard's own behaviour is tested in Harmony.Application.UnitTests; this applies it to this assembly.
    /// </summary>
    [Trait("Requirement", "FR-005")]
    public class RequirementTagGuardTests
    {
        [Fact]
        public void TestAssembly_EveryTestMethod_HasRequirementTag()
        {
            var testClasses = typeof(RequirementTagGuardTests).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsVisible);

            var violations = RequirementTagGuard.FindViolations(testClasses);

            violations.ShouldBeEmpty(
                "Every test needs [Trait(\"Requirement\", \"FR-nnn\")] or [Trait(\"Bug\", \"<issue>\")] on the method or its class: "
                + string.Join(", ", violations));
        }
    }
}
