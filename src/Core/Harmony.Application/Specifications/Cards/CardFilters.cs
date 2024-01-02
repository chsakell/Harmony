namespace Harmony.Application.Specifications.Cards
{
    public class CardFilters
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? BoardListId { get; set; }
        public Guid? BoardId { get; set; }
        public bool HasAttachments { get; set; }
        public bool CombineCriteria { get; set; }
    }
}
