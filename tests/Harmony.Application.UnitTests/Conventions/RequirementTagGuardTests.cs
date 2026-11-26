using Harmony.Tests.Shared;
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
        public void FindViolations_ClassTagValueNotRequirementId_ReturnsClassName()
        {
            var violations = RequirementTagGuard.FindViolations([typeof(WrongClassTagFixture)]);

            violations.ShouldHaveSingleItem().ShouldStartWith($"{nameof(WrongClassTagFixture)}:");
        }

        [Fact]
        public void FindViolations_BugTagOnly_ReturnsNoViolation()
        {
            var violations = RequirementTagGuard.FindViolations([typeof(BugTaggedFixture)]);

            violations.ShouldBeEmpty();
        }

        [Fact]
        public void FindViolations_BugTagValueNotIssueNumber_ReturnsMethodName()
        {
            var violations = RequirementTagGuard.FindViolations([typeof(WrongBugValueFixture)]);

            violations.ShouldHaveSingleItem()
                .ShouldStartWith($"{nameof(WrongBugValueFixture)}.{nameof(WrongBugValueFixture.TaggedWithBranchName)}");
        }

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

        // xUnit merges class and method traits, so a bad class tag reaches every method even when each method is tagged.
        [Trait("Requirement", "TODO")]
        public abstract class WrongClassTagFixture
        {
            [Fact]
            [Trait("Requirement", "FR-005")]
            public void TaggedMethod()
            {
            }

            [Fact]
            [Trait("Requirement", "FR-006")]
            public void OtherTaggedMethod()
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

        public abstract class BugTaggedFixture
        {
            [Fact]
            [Trait("Bug", "11")]
            public void TaggedWithIssue()
            {
            }
        }

        public abstract class WrongBugValueFixture
        {
            [Fact]
            [Trait("Bug", "bugfix/11-backlog")]
            public void TaggedWithBranchName()
            {
            }

            [Fact]
            [Trait("Bug", "11")]
            public void TaggedWithIssue()
            {
            }
        }
    }
}
