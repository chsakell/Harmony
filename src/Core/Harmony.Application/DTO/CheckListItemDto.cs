namespace Harmony.Application.DTO
{
    public class CheckListItemDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public Guid CheckListId { get; set; }
        public bool IsChecked { get; set; }
        public byte Position { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
