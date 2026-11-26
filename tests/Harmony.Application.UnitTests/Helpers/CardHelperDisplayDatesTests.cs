using System.Globalization;
using Harmony.Application.Helpers;
using Harmony.Application.UnitTests.TestSupport;
using Shouldly;
using Xunit;

namespace Harmony.Application.UnitTests.Helpers
{
    /// <summary>
    /// The short date range shown on a card. Every test fixes the culture, so the suite gives
    /// the same result on any machine.
    /// </summary>
    public class CardHelperDisplayDatesTests
    {
        [Fact]
        [Trait("Requirement", "FR-008")]
        public void DisplayDates_SameMonth_ShowsMonthAndBothDays()
        {
            using var _ = new CultureScope(CultureInfo.InvariantCulture);

            CardHelper.DisplayDates(new DateTime(2026, 3, 5), new DateTime(2026, 3, 19)).ShouldBe("Mar 05 - 19");
        }

        [Fact]
        [Trait("Requirement", "FR-009")]
        public void DisplayDates_DifferentMonths_ShowsBothMonthsWithoutYears()
        {
            using var _ = new CultureScope(CultureInfo.InvariantCulture);

            CardHelper.DisplayDates(new DateTime(2026, 3, 28), new DateTime(2026, 4, 3)).ShouldBe("Mar 28 - Apr 03");
        }

        [Fact]
        [Trait("Requirement", "FR-010")]
        public void DisplayDates_OnlyStartDate_ShowsStartDate()
        {
            using var _ = new CultureScope(CultureInfo.InvariantCulture);

            CardHelper.DisplayDates(new DateTime(2026, 3, 5), null).ShouldBe("Mar 05");
        }

        [Fact]
        [Trait("Requirement", "FR-010")]
        public void DisplayDates_OnlyDueDate_ShowsDueDate()
        {
            using var _ = new CultureScope(CultureInfo.InvariantCulture);

            CardHelper.DisplayDates(null, new DateTime(2026, 4, 3)).ShouldBe("Apr 03");
        }

        [Fact]
        [Trait("Requirement", "FR-011")]
        public void DisplayDates_NoDates_ReturnsEmpty()
        {
            using var _ = new CultureScope(CultureInfo.InvariantCulture);

            CardHelper.DisplayDates(null, null).ShouldBeEmpty();
        }

        [Fact]
        [Trait("Requirement", "FR-008")]
        public void DisplayDates_SameMonthDifferentYear_OmitsYear_KnownIssue()
        {
            // Known defect (spec concern 4a): only the month number is compared, so the year is ignored.
            using var _ = new CultureScope(CultureInfo.InvariantCulture);

            CardHelper.DisplayDates(new DateTime(2026, 1, 5), new DateTime(2027, 1, 10)).ShouldBe("Jan 05 - 10");
        }

        [Fact]
        [Trait("Requirement", "FR-008")]
        public void DisplayDates_GreekCulture_UsesGreekMonthName_KnownIssue()
        {
            // Known defect (spec concern 4b): month names follow the server's current culture,
            // which also reaches the emails sent by Notifications.
            var greek = CultureInfo.GetCultureInfo("el-GR");
            var greekMarch = greek.DateTimeFormat.AbbreviatedMonthNames[2];
            using var _ = new CultureScope(greek);

            var display = CardHelper.DisplayDates(new DateTime(2026, 3, 5), new DateTime(2026, 3, 19));

            display.ShouldBe($"{greekMarch} 05 - 19");
            display.ShouldNotStartWith("Mar");
        }
    }
}
