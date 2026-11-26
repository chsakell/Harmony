using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Persistence.DbContext;
using Microsoft.Extensions.DependencyInjection;

namespace Harmony.Api.IntegrationTests.Harness
{
    /// <summary>
    /// The IDs of one fixture: a workspace and a private board with one resource of each kind, and a second card as
    /// a link target (spec, Definitions, <i>Fixture</i>). Every fixture has new IDs.
    /// </summary>
    public sealed record Fixture(
        string OwnerId,
        Guid WorkspaceId,
        Guid BoardId,
        Guid ListId,
        Guid IssueTypeId,
        Guid SprintId,
        Guid CardId,
        Guid Card2Id,
        Guid ChecklistId,
        Guid ChecklistItemId,
        Guid CommentId,
        Guid LabelId,
        Guid LinkId,
        Guid AttachmentId);

    /// <summary>
    /// Writes fixtures directly through a host-scoped <see cref="HarmonyContext"/>, not through the endpoints under
    /// test. The owner is a member of the workspace and the Admin of the board; nobody else is a member of either.
    /// The entity builders are copied from Harmony.Infrastructure.IntegrationTests/Harness/TestData.cs.
    /// </summary>
    public static class FixtureFactory
    {
        public static async Task<Fixture> CreateAsync(IServiceProvider services, TestUser owner)
        {
            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = $"Workspace {Unique()}",
                Description = "Matrix fixture workspace",
                UserId = owner.Id,
                Status = WorkspaceStatus.Active,
            };
            var board = new Board
            {
                Id = Guid.NewGuid(),
                Title = $"Board {Unique()}",
                Description = "Matrix fixture board",
                UserId = owner.Id,
                WorkspaceId = workspace.Id,
                Visibility = BoardVisibility.Private,
                Type = BoardType.Scrum,
                Key = $"K{Unique()[..5].ToUpperInvariant()}",
            };
            var list = new BoardList
            {
                Id = Guid.NewGuid(),
                Title = $"List {Unique()}",
                BoardId = board.Id,
                UserId = owner.Id,
                Position = 0,
                Status = BoardListStatus.Active,
            };
            var issueType = new IssueType
            {
                Id = Guid.NewGuid(),
                Summary = "Task",
                Description = "Matrix fixture issue type",
                BoardId = board.Id,
            };
            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                Name = $"Sprint {Unique()}",
                BoardId = board.Id,
                Status = SprintStatus.Active,
            };
            var card = NewCard(owner, list, issueType, position: 0, serialNumber: 1);
            var card2 = NewCard(owner, list, issueType, position: 1, serialNumber: 2);
            var checkList = new CheckList
            {
                Id = Guid.NewGuid(),
                Title = "Checklist",
                CardId = card.Id,
                UserId = owner.Id,
            };
            var checkListItem = new CheckListItem
            {
                Id = Guid.NewGuid(),
                Description = "Item",
                CheckListId = checkList.Id,
                Position = 0,
            };
            var comment = new Comment { Id = Guid.NewGuid(), Text = "Comment", CardId = card.Id, UserId = owner.Id };
            var label = new Label { Id = Guid.NewGuid(), Title = $"Label {Unique()[..6]}", Colour = "#2196f3", BoardId = board.Id };
            var link = new Link
            {
                Id = Guid.NewGuid(),
                SourceCardId = card.Id,
                TargetCardId = card2.Id,
                UserId = owner.Id,
                Type = LinkType.RelatesTo,
            };
            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                FileName = $"{Unique()}.png",
                OriginalFileName = "screenshot.png",
                CardId = card.Id,
                UserId = owner.Id,
                Type = AttachmentType.CardImage,
            };

            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HarmonyContext>();
            context.AddRange(workspace, board, list, issueType, sprint, card, card2, checkList, checkListItem, comment, label, link, attachment);
            context.Add(new UserWorkspace { WorkspaceId = workspace.Id, UserId = owner.Id });
            context.Add(new UserBoard { BoardId = board.Id, UserId = owner.Id, Access = UserBoardAccess.Admin });
            await context.SaveChangesAsync();

            return new Fixture(owner.Id, workspace.Id, board.Id, list.Id, issueType.Id, sprint.Id, card.Id, card2.Id,
                checkList.Id, checkListItem.Id, comment.Id, label.Id, link.Id, attachment.Id);
        }

        private static Card NewCard(TestUser owner, BoardList list, IssueType issueType, short position, int serialNumber) =>
            new()
            {
                Id = Guid.NewGuid(),
                Title = $"Card {Unique()}",
                UserId = owner.Id,
                BoardListId = list.Id,
                IssueTypeId = issueType.Id,
                Status = CardStatus.Active,
                Position = position,
                SerialNumber = serialNumber,
            };

        private static string Unique() => Guid.NewGuid().ToString("N");
    }
}
