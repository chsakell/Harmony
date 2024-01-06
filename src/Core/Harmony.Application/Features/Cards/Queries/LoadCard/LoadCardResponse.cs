using Harmony.Application.DTO;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Queries.LoadCard
{
    /// <summary>
    /// Response for loading card
    /// </summary>
    public class LoadCardResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid BoardId { get; set; }
        public BoardType BoardType { get; set; }
        public string BoardTitle { get; set; }
        public string Description { get; set; }
        public int SerialNumber { get; set; }
        public string UserId { get; set; } // User created the card
        public BoardListDto BoardList { get; set; }
        public CardStatus Status { get; set; }
        public List<CheckListDto> CheckLists { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DueDateReminderType? DueDateReminderType { get; set; }
        public DateTime? ReminderDate { get; set; }
        public List<CardMemberDto> Members { get; set; }
        public List<LabelDto> Labels { get; set; }
        public SprintDto Sprint { get; set; }
        public short? StoryPoints { get; set; }
        public IssueTypeDto IssueType { get; set; }
        public List<CardDto> Children { get; set; }
        public bool IsChild { get; set; }
        public List<IssueTypeDto> IssueTypes { get; set; }
        public List<GetBoardListResponse> BoardLists { get; set; }
    }
}
