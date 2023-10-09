namespace Harmony.Application.DTO
{
    public class CardDto
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public Guid BoardListId { get; set; }
		public byte Position { get; set; } // position on the board list
		public int TotalItems { get; set; }
		public int TotalItemsCompleted { get; set; }
		public int TotalAttachments { get; set; }
		public List<LabelDto> Labels { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReminderDate { get; set; }
    }
}
