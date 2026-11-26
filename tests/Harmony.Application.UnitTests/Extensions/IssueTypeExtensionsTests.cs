using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Shared.Constants.Application;
using Shouldly;
using Xunit;

namespace Harmony.Application.UnitTests.Extensions
{
    /// <summary>
    /// <see cref="IssueTypeExtensions.Normal"/> keeps the issue types a card can be created with directly,
    /// dropping subtask types (summary contains <see cref="IssueTypesConstants.SUBTASK"/>, case-sensitive).
    /// </summary>
    [Trait("Requirement", "FR-012")]
    public class IssueTypeExtensionsTests
    {
        [Fact]
        public void Normal_ListWithSubTask_ReturnsListWithoutSubTask()
        {
            List<IssueTypeDto> issueTypes =
            [
                new() { Id = Guid.NewGuid(), Summary = "Story" },
                new() { Id = Guid.NewGuid(), Summary = "Bug" },
                new() { Id = Guid.NewGuid(), Summary = IssueTypesConstants.SUBTASK },
            ];

            var normal = issueTypes.Normal();

            normal.Select(issueType => issueType.Summary).ShouldBe(["Story", "Bug"]);
        }

        [Fact]
        public void Normal_NullList_ReturnsNull()
        {
            List<IssueTypeDto> issueTypes = null!;

            issueTypes.Normal().ShouldBeNull();
        }

        [Fact]
        public void Normal_TypeWithoutSummary_Throws_KnownIssue()
        {
            // Known defect (spec concern 4c): a type with no summary throws instead of being kept or skipped.
            List<IssueTypeDto> issueTypes = [new() { Id = Guid.NewGuid(), Summary = null! }];

            Should.Throw<NullReferenceException>(() => issueTypes.Normal());
        }
    }
}
