using Harmony.Domain.Enums;

namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Class to represent cards in lists
    /// </summary>
    public class Card : AuditableEntity<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
		public string UserId { get; set; } // User created the card
		public BoardList BoardList { get; set; }
        public Guid? BoardListId { get; set; }
        public short Position { get; set; } // position on the board list
        public List<Comment> Comments { get; set; }
        public List<CheckList> CheckLists { get; set; }
        public List<UserCard> Members { get; set; }
        public List<CardActivity> Activities { get; set; }
        public CardStatus Status { get; set; }
        public List<CardLabel> Labels { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DueDateReminderType? DueDateReminderType { get; set; }
        public DateTime? ReminderDate { get; set; }
        public List<Attachment> Attachments { get; set; }
        public int SerialNumber { get; set; }
        public IssueType IssueType { get; set; }
        public Guid? IssueTypeId { get; set; }
        public Guid? SprintId { get; set; }
        public Sprint Sprint { get; set; }
        public short? StoryPoints { get; set; }
        public DateTime? DateCompleted { get; set; }

        #region Child issue
        public List<Card> Children { get; set; }
        public Guid? ParentCardId { get; set; }
        public Card ParentCard { get; set; }
        #endregion
    }
}
