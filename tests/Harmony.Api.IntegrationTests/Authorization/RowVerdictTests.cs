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

        private const string AdminRow = "Admin (plain user) DELETE api/identity/role/{id}";

        private const string Permission = "Permissions.Roles.Delete";

        [Theory]
        [Trait("Requirement", "FR-110")]
        [Trait("Requirement", "FR-111")]
        [InlineData(AdminCaller.PlainUser)]
        [InlineData(AdminCaller.AllButRequired)]
        public void Admin_Refusing403_PassesForPlainAndAllBut(AdminCaller caller)
        {
            var verdict = RowVerdict.JudgeAdmin(caller, Reached(403), AdminRow, Permission);

            RowVerdict.Classify(Rule.Admin, Reached(403)).ShouldBe(Outcome.Refused);
            verdict.Passed.ShouldBeTrue(verdict.Message);
        }

        [Theory]
        [Trait("Requirement", "FR-110")]
        [Trait("Requirement", "FR-111")]
        [InlineData(AdminCaller.PlainUser, 200)]
        [InlineData(AdminCaller.PlainUser, 404)]
        [InlineData(AdminCaller.AllButRequired, 400)]
        [InlineData(AdminCaller.AllButRequired, 500)]
        public void Admin_LetIn_FailsForPlainAndAllBut(AdminCaller caller, int status)
        {
            var verdict = RowVerdict.JudgeAdmin(caller, Reached(status), AdminRow, Permission);

            verdict.Passed.ShouldBeFalse();
            verdict.Message.ShouldContain(AdminRow);
            verdict.Message.ShouldContain(status.ToString(System.Globalization.CultureInfo.InvariantCulture));
            verdict.Message.ShouldContain("expected 403");
            verdict.Message.ShouldContain(Permission);
        }

        [Fact]
        [Trait("Requirement", "FR-112")]
        public void Admin_OnlyRequired403_FailsNamingPermission()
        {
            var verdict = RowVerdict.JudgeAdmin(AdminCaller.OnlyRequired, Reached(403), AdminRow, Permission);

            verdict.Passed.ShouldBeFalse();
            verdict.Message.ShouldContain("403");
            verdict.Message.ShouldContain(Permission);
        }

        [Theory]
        [Trait("Requirement", "FR-112")]
        [InlineData(200)]
        [InlineData(400)]
        [InlineData(500)]
        public void Admin_OnlyRequiredOtherStatus_IsLetIn(int status)
        {
            var verdict = RowVerdict.JudgeAdmin(AdminCaller.OnlyRequired, Reached(status), AdminRow, Permission);

            RowVerdict.Classify(Rule.Admin, Reached(status)).ShouldBe(Outcome.LetIn);
            verdict.Passed.ShouldBeTrue(verdict.Message);
        }

        [Theory]
        [Trait("Requirement", "FR-110")]
        [Trait("Requirement", "FR-111")]
        [Trait("Requirement", "FR-112")]
        [InlineData(AdminCaller.PlainUser, 401, true)]
        [InlineData(AdminCaller.AllButRequired, 405, true)]
        [InlineData(AdminCaller.OnlyRequired, 401, true)]
        [InlineData(AdminCaller.OnlyRequired, 200, false)]
        [InlineData(AdminCaller.PlainUser, 403, false)]
        public void Admin_401Or405OrNotReached_IsInconclusive(AdminCaller caller, int status, bool reached)
        {
            var response = new RowResponse(status, reached, reached ? "DELETE api/identity/role/{id}" : "/{*path:nonfile} (fallback)", "body");

            var verdict = RowVerdict.JudgeAdmin(caller, response, AdminRow, Permission);

            RowVerdict.Classify(Rule.Admin, response).ShouldBe(Outcome.Inconclusive);
            verdict.Passed.ShouldBeFalse();
            verdict.Message.ShouldContain("inconclusive");
        }
    }
}
