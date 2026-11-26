using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Persistence.DbContext;
using Harmony.Persistence.Identity;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// Seed builders for relational tests (the test data rule): every call inserts new rows with unique IDs, names
    /// and keys, through its own context, and returns the entity with its keys set. Each builder sets every required
    /// column. Pass only what the test is about; the rest gets a valid default.
    /// </summary>
    public sealed class TestData(TestDatabase database)
    {
        public Task<HarmonyUser> UserAsync()
        {
            var id = Guid.NewGuid().ToString();
            var userName = $"user-{id[..8]}";
            return AddAsync(new HarmonyUser
            {
                Id = id,
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                Email = $"{userName}@example.test",
                NormalizedEmail = $"{userName}@example.test".ToUpperInvariant(),
                FirstName = "Test",
                LastName = userName,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
            });
        }

        public Task<Workspace> WorkspaceAsync(HarmonyUser owner, WorkspaceStatus status = WorkspaceStatus.Active) =>
            AddAsync(new Workspace
            {
                Id = Guid.NewGuid(),
                Name = $"Workspace {Unique()}",
                Description = "Integration test workspace",
                UserId = owner.Id,
                Status = status,
            });

        public Task<UserWorkspace> WorkspaceMemberAsync(Workspace workspace, HarmonyUser user) =>
            AddAsync(new UserWorkspace { WorkspaceId = workspace.Id, UserId = user.Id });

        public Task<Board> BoardAsync(Workspace workspace, HarmonyUser owner,
            BoardVisibility visibility = BoardVisibility.Workspace, string? key = null, BoardType type = BoardType.Kanban) =>
            AddAsync(new Board
            {
                Id = Guid.NewGuid(),
                Title = $"Board {Unique()}",
                Description = "Integration test board",
                UserId = owner.Id,
                WorkspaceId = workspace.Id,
                Visibility = visibility,
                Type = type,
                Key = key ?? $"K{Unique()[..5].ToUpperInvariant()}",
            });

        public Task<UserBoard> BoardMemberAsync(Board board, HarmonyUser user) =>
            AddAsync(new UserBoard { BoardId = board.Id, UserId = user.Id, Access = UserBoardAccess.Member });

        public Task<BoardList> ListAsync(Board board, HarmonyUser owner, short position = 0) =>
            AddAsync(new BoardList
            {
                Id = Guid.NewGuid(),
                Title = $"List {Unique()}",
                BoardId = board.Id,
                UserId = owner.Id,
                Position = position,
                Status = BoardListStatus.Active,
            });

        public Task<IssueType> IssueTypeAsync(Board board, string summary = "Task") =>
            AddAsync(new IssueType
            {
                Id = Guid.NewGuid(),
                Summary = summary,
                Description = "Integration test issue type",
                BoardId = board.Id,
            });

        public Task<Sprint> SprintAsync(Board board, SprintStatus status = SprintStatus.Active) =>
            AddAsync(new Sprint
            {
                Id = Guid.NewGuid(),
                Name = $"Sprint {Unique()}",
                BoardId = board.Id,
                Status = status,
            });

        /// <summary>
        /// A card belongs to a board through its list or, in the backlog (no list), through its issue type.
        /// </summary>
        public Task<Card> CardAsync(HarmonyUser creator, BoardList? list = null, IssueType? issueType = null,
            CardStatus status = CardStatus.Active, short position = 0, Card? parent = null, Sprint? sprint = null,
            string? title = null, int serialNumber = 1) =>
            AddAsync(new Card
            {
                Id = Guid.NewGuid(),
                Title = title ?? $"Card {Unique()}",
                UserId = creator.Id,
                BoardListId = list?.Id,
                IssueTypeId = issueType?.Id,
                Status = status,
                Position = position,
                ParentCardId = parent?.Id,
                SprintId = sprint?.Id,
                SerialNumber = serialNumber,
            });

        public Task<Label> LabelAsync(Board board) =>
            AddAsync(new Label { Id = Guid.NewGuid(), Title = $"Label {Unique()[..6]}", Colour = "#2196f3", BoardId = board.Id });

        public Task<CardLabel> CardLabelAsync(Card card, Label label) =>
            AddAsync(new CardLabel { CardId = card.Id, LabelId = label.Id });

        public Task<UserCard> CardMemberAsync(Card card, HarmonyUser user) =>
            AddAsync(new UserCard { CardId = card.Id, UserId = user.Id });

        public async Task<CheckList> CheckListAsync(Card card, HarmonyUser owner, int items, int checkedItems)
        {
            var checkList = await AddAsync(new CheckList
            {
                Id = Guid.NewGuid(),
                Title = "Checklist",
                CardId = card.Id,
                UserId = owner.Id,
            });

            for (var i = 0; i < items; i++)
            {
                await AddAsync(new CheckListItem
                {
                    Id = Guid.NewGuid(),
                    Description = $"Item {i}",
                    CheckListId = checkList.Id,
                    IsChecked = i < checkedItems,
                    Position = (byte)i,
                });
            }

            return checkList;
        }

        public Task<Comment> CommentAsync(Card card, HarmonyUser author) =>
            AddAsync(new Comment { Id = Guid.NewGuid(), Text = "Comment", CardId = card.Id, UserId = author.Id });

        public Task<Link> LinkAsync(Card source, Card target, HarmonyUser author) =>
            AddAsync(new Link
            {
                Id = Guid.NewGuid(),
                SourceCardId = source.Id,
                TargetCardId = target.Id,
                UserId = author.Id,
                Type = LinkType.RelatesTo,
            });

        public Task<Attachment> AttachmentAsync(Card card, HarmonyUser owner) =>
            AddAsync(new Attachment
            {
                Id = Guid.NewGuid(),
                FileName = $"{Unique()}.png",
                OriginalFileName = "screenshot.png",
                CardId = card.Id,
                UserId = owner.Id,
                Type = AttachmentType.CardImage,
            });

        private async Task<TEntity> AddAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            await using var context = database.CreateContext<HarmonyContext>();
            context.Add(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        private static string Unique() => Guid.NewGuid().ToString("N");
    }
}
