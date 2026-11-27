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
            var rows = MatrixRows.AnonymousRefused().Concat(MatrixRows.AnonymousPublic()).Concat(MatrixRows.AnonymousKnownGaps())
                .Where(row => !MatrixRows.IsPlaceholder(row.Data.Item1))
                .ToList();

            rows.Count.ShouldBe(lines.Count);
            rows.Select(row => row.TestDisplayName).ShouldBeUnique();
            rows.ShouldContain(row => row.TestDisplayName == "Anonymous DELETE api/identity/role/{id}");
        }

        [Fact]
        [Trait("Requirement", "FR-110")]
        [Trait("Requirement", "FR-111")]
        [Trait("Requirement", "FR-112")]
        public void AdminRows_OneRowPerAdministrationLinePerCaller()
        {
            var administration = MatrixFile.Load().Lines.Where(line => line.Scope == Scope.Administration).ToList();
            administration.Count.ShouldBe(8, "the spec's Administration permissions table");

            (string Name, List<TheoryDataRow<string, string>> Rows)[] sources =
            [
                ("plain user", MatrixRows.AdminPlainUser().ToList()),
                ("all but required", MatrixRows.AdminAllButRequired().ToList()),
                ("only required", MatrixRows.AdminOnlyRequired().ToList()),
            ];

            foreach (var (name, rows) in sources)
            {
                rows.Select(row => (row.Data.Item1, row.Data.Item2)).ShouldBe(administration.Select(line => (line.Method, line.Route)), ignoreOrder: true);
                rows.Select(row => row.TestDisplayName).ShouldBeUnique();
                rows.ShouldContain(row => row.TestDisplayName == $"Admin ({name}) DELETE api/identity/role/{{id}}");
            }
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
