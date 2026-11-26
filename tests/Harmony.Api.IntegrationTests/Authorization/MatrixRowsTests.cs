using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// Theory rows come from the checked-in file, so they can be listed without Docker, and each one is named after
    /// its rule, method and route (FR-014). Pure code: no host.
    /// </summary>
    public class MatrixRowsTests
    {
        [Fact]
        [Trait("Requirement", "FR-014")]
        public void AnonymousRows_CoverEveryLineOnceWithDisplayName()
        {
            var lines = MatrixFile.Load().Lines;
            var rows = MatrixRows.AnonymousRefused().Concat(MatrixRows.AnonymousPublic()).Concat(MatrixRows.AnonymousKnownGaps()).ToList();

            rows.Count.ShouldBe(lines.Count);
            rows.Select(row => row.TestDisplayName).ShouldBeUnique();
            rows.ShouldContain(row => row.TestDisplayName == "Anonymous DELETE api/identity/role/{id}");
        }

        [Fact]
        [Trait("Requirement", "FR-040")]
        public void Rows_FilterWithNoLine_YieldsOnePlaceholder()
        {
            var rows = MatrixRows.For(Rule.NonMember, Mark.KnownGap, []).ToList();

            var row = rows.ShouldHaveSingleItem();
            MatrixRows.IsPlaceholder(row.Data.Item1).ShouldBeTrue();
            row.TestDisplayName.ShouldBe("Non-member (no line: every endpoint is marked)");
        }
    }
}
