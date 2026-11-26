using Shouldly;
using Xunit;

namespace Harmony.Application.UnitTests.Conventions
{
    [Trait("Requirement", "FR-005")]
    public class RequirementTagGuardTests
    {
        [Fact]
        public void FindViolations_MethodWithoutTag_ReturnsMethodName()
        {
            var violations = RequirementTagGuard.FindViolations([typeof(UntaggedFixture)]);

            violations.ShouldHaveSingleItem()
                .ShouldStartWith($"{nameof(UntaggedFixture)}.{nameof(UntaggedFixture.Untagged)}");
        }

        [Fact]
        public void FindViolations_TagValueNotRequirementId_ReturnsMethodName()
        {
            var violations = RequirementTagGuard.FindViolations([typeof(WrongValueFixture)]);

            violations.ShouldHaveSingleItem()
                .ShouldStartWith($"{nameof(WrongValueFixture)}.{nameof(WrongValueFixture.TaggedTodo)}");
        }

        [Fact]
        public void FindViolations_ClassLevelTag_ReturnsNoViolation()
        {
            var violations = RequirementTagGuard.FindViolations([typeof(ClassTaggedFixture)]);

            violations.ShouldBeEmpty();
        }

        [Fact]
        public void TestAssembly_EveryTestMethod_HasRequirementTag()
        {
            var testClasses = typeof(RequirementTagGuardTests).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsVisible);

            var violations = RequirementTagGuard.FindViolations(testClasses);

            violations.ShouldBeEmpty(
                "Every test needs [Trait(\"Requirement\", \"FR-nnn\")] on the method or its class: "
                + string.Join(", ", violations));
        }

        // Fixtures: abstract, so xUnit does not run them and the assembly guard skips them.

        public abstract class UntaggedFixture
        {
            [Fact]
            public void Untagged()
            {
            }

            [Trait("Requirement", "FR-005")]
            [Theory]
            [InlineData(1)]
            public void TaggedTheory(int value)
            {
                _ = value;
            }

            public void NotATest()
            {
            }
        }

        public abstract class WrongValueFixture
        {
            [Fact]
            [Trait("Requirement", "TODO")]
            public void TaggedTodo()
            {
            }

            [Fact]
            [Trait("Requirement", "NFR-002")]
            public void TaggedNfr()
            {
            }
        }

        [Trait("Requirement", "FR-005")]
        public abstract class ClassTaggedFixture
        {
            [Fact]
            public void InheritsClassTag()
            {
            }
        }
    }
}
