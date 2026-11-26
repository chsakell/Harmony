using Harmony.Application.DTO.Automation;
using Harmony.Domain.Automation;
using Harmony.Domain.Enums;
using Harmony.Domain.Enums.Automations;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Harmony.Infrastructure.Repositories;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.MongoDb
{
    [Collection(MongoDbCollection.Name)]
    [Trait("Requirement", "FR-050")]
    public class AutomationRepositoryTests(MongoDbFixture mongo) : IAsyncLifetime
    {
        private readonly AutomationRepository repository = new(mongo.Client);

        public async ValueTask InitializeAsync() => await mongo.DropDatabasesAsync();

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        [Fact]
        public async Task AutomationRepository_RuleLifecycle_ReturnsLastSavedState()
        {
            var boardId = Guid.NewGuid();
            var otherBoardId = Guid.NewGuid();
            var otherRule = Rule(otherBoardId, "Other board's rule");
            await repository.CreateAsync(otherRule);
            var rule = Rule(boardId, "Sum up story points");

            await repository.CreateAsync(rule);
            (await RulesAsync(boardId)).ShouldHaveSingleItem().Name.ShouldBe("Sum up story points");

            rule.Name = "Sum up story points of stories";
            rule.IssueTypes = ["Story"];
            (await repository.ReplaceAsync(rule)).ShouldBeTrue();
            var replaced = (await RulesAsync(boardId)).ShouldHaveSingleItem();
            replaced.Name.ShouldBe("Sum up story points of stories");
            replaced.IssueTypes.ShouldBe(["Story"]);

            (await repository.ChangeStatusAsync(rule.Id, enabled: false)).ShouldBeTrue();
            (await RulesAsync(boardId)).ShouldHaveSingleItem().Enabled.ShouldBeFalse();

            (await repository.Remove(rule.Id)).ShouldBeTrue();
            (await RulesAsync(boardId)).ShouldBeEmpty();

            (await RulesAsync(otherBoardId)).ShouldHaveSingleItem().Id.ShouldBe(otherRule.Id);
        }

        [Fact]
        public async Task AutomationRepository_Templates_ListedInFullAndByNotificationType()
        {
            var moved = Template("Sync parent and child issues", NotificationType.CardMoved);
            var created = Template("Smart auto assign", NotificationType.CardCreated);
            var movedToo = Template("Sum up story points", NotificationType.CardMoved);
            await repository.CreateTemplates([moved, created, movedToo]);

            var all = await repository.GetTemplates();
            var whenMoved = await repository.GetTemplates(NotificationType.CardMoved);

            all.Select(template => template.Name).ShouldBe([moved.Name, created.Name, movedToo.Name], ignoreOrder: true);
            whenMoved.Select(template => template.Name).ShouldBe([moved.Name, movedToo.Name], ignoreOrder: true);
        }

        private async Task<List<SumUpStoryPointsAutomationDto>> RulesAsync(Guid boardId) =>
            (await repository.GetAutomations<SumUpStoryPointsAutomationDto>(AutomationType.SumUpStoryPoints, boardId)).ToList();

        private static SumUpStoryPointsAutomationDto Rule(Guid boardId, string name) => new()
        {
            Name = name,
            Description = "Integration test rule",
            Type = AutomationType.SumUpStoryPoints,
            BoardId = boardId.ToString(),
            Enabled = true,
            IssueTypes = ["Task"],
        };

        private static AutomationTemplate Template(string name, NotificationType notificationType) => new()
        {
            Name = name,
            Summary = $"{name} summary",
            Type = AutomationType.SyncParentAndChildIssues,
            Enabled = true,
            NotificationTypes = [notificationType],
        };
    }
}
