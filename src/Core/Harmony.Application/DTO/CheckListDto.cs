namespace Harmony.Application.DTO
{
    public class CheckListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid CardId { get; set; }
        public List<CheckListItemDto> Items { get; set; }
        public byte Position { get; set; }
    }
}
