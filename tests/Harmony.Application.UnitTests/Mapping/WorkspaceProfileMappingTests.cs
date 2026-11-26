using AutoMapper;
using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Shouldly;
using Xunit;

namespace Harmony.Application.UnitTests.Mapping
{
    /// <summary>
    /// Input and output pairs for the six mappings with custom rules in <c>WorkspaceProfile</c>.
    /// Expected values are written by hand, so the tests survive replacing AutoMapper (R-14).
    /// </summary>
    [Trait("Requirement", "FR-015")]
    public class WorkspaceProfileMappingTests
    {
        private readonly IMapper _mapper = MappingConfigurationFactory.CreateMapper();

        [Fact]
        public void Map_BoardToGetWorkspaceBoardResponse_CountsUsers()
        {
            var board = new Board
            {
                Id = Guid.NewGuid(),
                Title = "Platform",
                Users = [new() { UserId = "u1" }, new() { UserId = "u2" }, new() { UserId = "u3" }],
            };

            var response = _mapper.Map<GetWorkspaceBoardResponse>(board);

            response.TotalUsers.ShouldBe(3);
        }

        [Fact]
        public void Map_CardToCardDto_ComputesTotalsAndLabels()
        {
            var card = NewCard();
            card.CheckLists =
            [
                new() { Items = [new() { IsChecked = true }, new() { IsChecked = false }] },
                new() { Items = [new() { IsChecked = false }] },
                new() { Items = null! },
            ];
            card.Attachments = [new(), new()];
            card.Comments = [new(), new(), new(), new()];
            card.Children = [new(), new(), new(), new(), new()];
            card.Labels =
            [
                new() { Label = new Label { Id = Guid.NewGuid(), Title = "Backend", Colour = "#0000ff" } },
                new() { Label = new Label { Id = Guid.NewGuid(), Title = "Urgent", Colour = "#ff0000" } },
            ];
            card.BoardList = new BoardList { Title = "Released", CardStatus = BoardListCardStatus.DONE };

            var dto = _mapper.Map<CardDto>(card);

            dto.ShouldSatisfyAllConditions(
                () => dto.TotalItems.ShouldBe(3),
                () => dto.TotalItemsCompleted.ShouldBe(1),
                () => dto.TotalAttachments.ShouldBe(2),
                () => dto.TotalComments.ShouldBe(4),
                () => dto.TotalChildren.ShouldBe(5),
                () => dto.Labels.Select(label => label.Title).ShouldBe(["Backend", "Urgent"]),
                () => dto.Labels.Select(label => label.Colour).ShouldBe(["#0000ff", "#ff0000"]),
                () => dto.BoardList.ShouldBe("Released"),
                () => dto.Completed.ShouldBe(true));
        }

        [Fact]
        public void Map_CardWithoutBoardList_SetsBoardListEmptyAndCompletedNull()
        {
            var card = NewCard();
            card.BoardList = null!;

            var dto = _mapper.Map<CardDto>(card);

            dto.ShouldSatisfyAllConditions(
                () => dto.BoardList.ShouldBe(string.Empty),
                () => dto.Completed.ShouldBeNull(),
                () => dto.TotalAttachments.ShouldBe(0),
                () => dto.TotalComments.ShouldBe(0),
                () => dto.TotalChildren.ShouldBe(0));
        }

        [Fact]
        public void Map_CardToLoadCardResponse_UsesIssueTypeBoardWhenPresent()
        {
            var issueTypeBoardId = Guid.NewGuid();
            var card = NewCard();
            card.ParentCardId = Guid.NewGuid();
            card.BoardList = new BoardList
            {
                BoardId = Guid.NewGuid(),
                Board = new Board { Title = "List board", Type = BoardType.Scrum },
            };
            card.IssueType = new IssueType
            {
                Summary = "Story",
                BoardId = issueTypeBoardId,
                Board = new Board { Id = issueTypeBoardId, Title = "Issue type board", Type = BoardType.Kanban },
            };
            card.Labels = [new() { Label = new Label { Title = "Backend" } }];

            var response = _mapper.Map<LoadCardResponse>(card);

            response.ShouldSatisfyAllConditions(
                () => response.BoardId.ShouldBe(issueTypeBoardId),
                () => response.BoardTitle.ShouldBe("Issue type board"),
                () => response.BoardType.ShouldBe(BoardType.Scrum),
                () => response.Labels.Select(label => label.Title).ShouldBe(["Backend"]),
                () => response.IsChild.ShouldBeTrue());
        }

        [Fact]
        public void Map_CardToLoadCardResponse_UsesBoardListBoardWithoutIssueType()
        {
            var listBoardId = Guid.NewGuid();
            var card = NewCard();
            card.IssueType = null!;
            card.ParentCardId = null;
            card.BoardList = new BoardList
            {
                BoardId = listBoardId,
                Board = new Board { Id = listBoardId, Title = "List board", Type = BoardType.Kanban },
            };

            var response = _mapper.Map<LoadCardResponse>(card);

            response.ShouldSatisfyAllConditions(
                () => response.BoardId.ShouldBe(listBoardId),
                () => response.BoardTitle.ShouldBe("List board"),
                () => response.BoardType.ShouldBe(BoardType.Kanban),
                () => response.IsChild.ShouldBeFalse());
        }

        [Fact]
        public void Map_CardLabelToLabelDto_TakesLabelTitle()
        {
            var cardLabel = new CardLabel { Label = new Label { Title = "Backend" } };

            var dto = _mapper.Map<LabelDto>(cardLabel);

            dto.Title.ShouldBe("Backend");
        }

        [Fact]
        public void Map_AttachmentToAttachmentDto_BuildsFilesUrl()
        {
            var attachment = new Attachment { Type = AttachmentType.CardDocument, FileName = "4f2a-report.pdf" };

            var dto = _mapper.Map<AttachmentDto>(attachment);

            dto.Url.ShouldBe("files/Cards/Documents/4f2a-report.pdf");
        }

        [Fact]
        public void Map_UserCardToCardMemberDto_UsesUserIdAsId()
        {
            var userCard = new UserCard { UserId = "user-7", CardId = Guid.NewGuid() };

            var dto = _mapper.Map<CardMemberDto>(userCard);

            dto.Id.ShouldBe("user-7");
        }

        /// <summary>A card with empty collections, so each test sets only the inputs its rules read.</summary>
        private static Card NewCard() => new()
        {
            Id = Guid.NewGuid(),
            Title = "Fix login",
            CheckLists = [],
            Labels = [],
            Attachments = [],
            Comments = [],
            Children = [],
            Members = [],
            Links = [],
        };
    }
}
