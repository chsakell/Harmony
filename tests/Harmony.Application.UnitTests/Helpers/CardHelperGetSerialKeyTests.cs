using Harmony.Application.Helpers;
using Shouldly;
using Xunit;

namespace Harmony.Application.UnitTests.Helpers
{
    /// <summary>
    /// Card key extraction from free text (commit messages, branch names), for example <c>HAR-42</c>.
    /// Allowed neighbours on both sides of a key: start or end of text, space, <c>_</c>, <c>-</c> and <c>/</c>.
    /// </summary>
    public class CardHelperGetSerialKeyTests
    {
        [Theory]
        [Trait("Requirement", "FR-006")]
        [InlineData("HAR-42 fix login", "HAR-42")]
        [InlineData("feature/HAR-42_login", "HAR-42")]
        [InlineData("fixes abc-7 and HAR-9", "abc-7")]
        public void GetSerialKey_KeyWithAllowedNeighbours_ReturnsFirstKey(string text, string expected)
        {
            CardHelper.GetSerialKey(text).ShouldBe(expected);
        }

        [Theory]
        [Trait("Requirement", "FR-007")]
        [InlineData("no key here")]
        [InlineData("#HAR-42")]
        [InlineData("see HARMONY-12")]
        [InlineData("HAR-42x")]
        [InlineData("AB-1")]
        public void GetSerialKey_NoKeyWithAllowedNeighbours_ReturnsNull(string text)
        {
            CardHelper.GetSerialKey(text).ShouldBeNull();
        }

        [Fact]
        [Trait("Requirement", "FR-007")]
        public void GetSerialKey_KeyFollowedByPeriod_ReturnsNull_KnownIssue()
        {
            // Known defect (spec concern 4f): '.' is not an allowed neighbour, so "Fixes HAR-42." finds no key.
            CardHelper.GetSerialKey("Fixes HAR-42.").ShouldBeNull();
        }

        [Fact]
        [Trait("Requirement", "FR-007")]
        public void GetSerialKey_NullText_Throws_KnownIssue()
        {
            // Known defect (spec concern 4d): no text throws instead of returning no key.
            Should.Throw<ArgumentNullException>(() => CardHelper.GetSerialKey(null!));
        }
    }
}
