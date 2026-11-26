using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// How a row's response and its line's mark become pass or fail. Pure code: no host.
    /// </summary>
    public class RowVerdictTests
    {
        private const string Row = "Anonymous DELETE api/identity/role/{id}";

        private static RowResponse Reached(int status, string body = "") => new(status, ReachedEndpoint: true, "DELETE api/identity/role/{id}", body);

        [Fact]
        [Trait("Requirement", "FR-041")]
        public void KnownGap_Refused_FailsSayingRemoveMark()
        {
            var verdict = RowVerdict.Judge(Rule.Anonymous, Mark.KnownGap, Reached(401), Row);

            verdict.Passed.ShouldBeFalse();
            verdict.Message.ShouldContain("remove the known-gap mark");
            verdict.Message.ShouldContain("gap R-07");
            verdict.Message.ShouldContain(Row);
        }

        [Theory]
        [Trait("Requirement", "FR-040")]
        [InlineData(200)]
        [InlineData(500)]
        public void KnownGap_LetIn_Passes(int status)
        {
            RowVerdict.Judge(Rule.Anonymous, Mark.KnownGap, Reached(status), Row).Passed.ShouldBeTrue();
        }

        [Theory]
        [Trait("Requirement", "FR-015")]
        [InlineData(Mark.Refused)]
        [InlineData(Mark.KnownGap)]
        [InlineData(Mark.NotApplicable)]
        public void NotReached_IsInconclusive(Mark mark)
        {
            var response = new RowResponse(200, ReachedEndpoint: false, "/{*path:nonfile} (fallback)", "<html>");

            var verdict = RowVerdict.Judge(Rule.Anonymous, mark, response, Row);

            RowVerdict.Classify(Rule.Anonymous, response).ShouldBe(Outcome.Inconclusive);
            verdict.Passed.ShouldBeFalse();
            verdict.Message.ShouldContain("inconclusive");
            verdict.Message.ShouldContain("/{*path:nonfile} (fallback)");
        }

        [Fact]
        [Trait("Requirement", "FR-015")]
        public void Status405_IsInconclusiveForBothRules()
        {
            RowVerdict.Classify(Rule.Anonymous, Reached(405)).ShouldBe(Outcome.Inconclusive);
            RowVerdict.Classify(Rule.NonMember, Reached(405)).ShouldBe(Outcome.Inconclusive);
        }

        [Theory]
        [Trait("Requirement", "FR-034")]
        [InlineData(400)]
        [InlineData(401)]
        [InlineData(405)]
        [InlineData(415)]
        public void NonMember_400_IsInconclusiveWithBody(int status)
        {
            var response = Reached(status, "{\"errors\":{\"CardId\":[\"The CardId field is required.\"]}}");

            var verdict = RowVerdict.Judge(Rule.NonMember, Mark.KnownGap, response, Row, owner: Reached(200));

            RowVerdict.Classify(Rule.NonMember, response).ShouldBe(Outcome.Inconclusive);
            verdict.Passed.ShouldBeFalse();
            verdict.Message.ShouldContain("inconclusive");
            verdict.Message.ShouldContain("The CardId field is required.");
        }

        [Theory]
        [Trait("Requirement", "FR-030")]
        [InlineData(403)]
        [InlineData(404)]
        public void NonMember_403Or404_IsRefused(int status)
        {
            RowVerdict.Judge(Rule.NonMember, Mark.Refused, Reached(status), Row, owner: Reached(200)).Passed.ShouldBeTrue();
        }

        [Theory]
        [Trait("Requirement", "FR-033")]
        [InlineData(401)]
        [InlineData(403)]
        [InlineData(404)]
        public void NonMember_OwnerRefused_IsInconclusive(int ownerStatus)
        {
            var verdict = RowVerdict.Judge(Rule.NonMember, Mark.KnownGap, Reached(200), Row, owner: Reached(ownerStatus, "owner body"));

            verdict.Passed.ShouldBeFalse();
            verdict.Message.ShouldContain("owner");
            verdict.Message.ShouldContain(ownerStatus.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        [Fact]
        [Trait("Requirement", "FR-033")]
        public void NonMember_WithoutOwnerResponse_Throws()
        {
            Should.Throw<ArgumentNullException>(() => RowVerdict.Judge(Rule.NonMember, Mark.Refused, Reached(403), Row));
        }

        [Fact]
        [Trait("Requirement", "FR-020")]
        public void Refused_LetIn_FailsNamingStatus()
        {
            var verdict = RowVerdict.Judge(Rule.Anonymous, Mark.Refused, Reached(200), Row);

            verdict.Passed.ShouldBeFalse();
            verdict.Message.ShouldContain("200");
            verdict.Message.ShouldContain("expected 401");
        }

        [Theory]
        [Trait("Requirement", "FR-021")]
        [InlineData(200, true)]
        [InlineData(400, true)]
        [InlineData(401, false)]
        public void Public_IsNot401(int status, bool passes)
        {
            RowVerdict.Judge(Rule.Anonymous, Mark.NotApplicable, Reached(status), Row).Passed.ShouldBe(passes);
        }
    }
}
