using Harmony.Domain.Enums;

namespace Harmony.Application.DTO
{
    public class CardDto
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public Guid BoardListId { get; set; }
		public short Position { get; set; } // position on the board list
		public int TotalItems { get; set; }
		public int TotalItemsCompleted { get; set; }
		public int TotalAttachments { get; set; }
		public List<LabelDto> Labels { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DueDateReminderType? DueDateReminderType { get; set; }
        public DateTime? ReminderDate { get; set; }
		public List<CardMemberDto> Members { get; set; }
        public int SerialNumber { get; set; }
        public CardStatus Status { get; set; }
		public IssueTypeDto IssueType { get; set; }
		public SprintDto Sprint { get; set; }
        public bool IsUpdating { get; set; }
		public int TotalComments { get; set; }
		public int TotalChildren { get; set; }
        public Guid? ParentCardId { get; set; }
        public short? StoryPoints { get; set; }
    }
}
