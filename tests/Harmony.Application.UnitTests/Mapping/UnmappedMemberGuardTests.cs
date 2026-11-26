using Shouldly;
using Xunit;

namespace Harmony.Application.UnitTests.Mapping
{
    public class UnmappedMemberGuardTests
    {
        private static readonly (string TypeMap, string Member)[] Baseline =
        [
            ("Card -> CardDto", "BoardKey"),
            ("Card -> CardDto", "Hidden"),
            ("Link -> LinkDetailsDto", "TargetCardTitle"),
        ];

        [Fact]
        [Trait("Requirement", "FR-016")]
        public void Compare_NewUnmappedMember_FailsNamingTypeMapAndMember()
        {
            var actual = Baseline.Append(("Card -> LoadCardResponse", "Watchers"));

            var comparison = UnmappedMemberGuard.Compare(actual, Baseline);

            comparison.ShouldSatisfyAllConditions(
                () => comparison.MatchesBaseline.ShouldBeFalse(),
                () => comparison.NewUnmapped.ShouldHaveSingleItem().ShouldBe(("Card -> LoadCardResponse", "Watchers")),
                () => comparison.NowMapped.ShouldBeEmpty(),
                () => comparison.Message.ShouldContain("Card -> LoadCardResponse: Watchers"));
        }

        [Fact]
        [Trait("Requirement", "FR-017")]
        public void Compare_BaselineEntryNowMapped_FailsAskingToRemoveEntry()
        {
            var actual = Baseline.Where(entry => entry.Member != "Hidden");

            var comparison = UnmappedMemberGuard.Compare(actual, Baseline);

            comparison.ShouldSatisfyAllConditions(
                () => comparison.MatchesBaseline.ShouldBeFalse(),
                () => comparison.NewUnmapped.ShouldBeEmpty(),
                () => comparison.NowMapped.ShouldHaveSingleItem().ShouldBe(("Card -> CardDto", "Hidden")),
                () => comparison.Message.ShouldContain("Card -> CardDto: Hidden"),
                () => comparison.Message.ShouldContain("remove", Case.Insensitive));
        }

        [Fact]
        [Trait("Requirement", "FR-016")]
        [Trait("Requirement", "FR-017")]
        public void Configuration_UnmappedMembers_MatchBaseline()
        {
            var actual = UnmappedMemberGuard.ReadUnmappedMembers(MappingConfigurationFactory.Configuration);

            var comparison = UnmappedMemberGuard.Compare(actual, UnmappedMemberBaseline.Entries);

            comparison.MatchesBaseline.ShouldBeTrue(comparison.Message);
            actual.Count.ShouldBe(28);
            actual.Select(entry => entry.TypeMap).Distinct().Count().ShouldBe(12);
        }
    }
}
