namespace Harmony.Application.DTO.Search
{
    public class IndexedCard
    {
        public string ObjectID { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public Guid BoardId { get; set; }
        public Guid ListId { get; set; }
        public string IssueType { get; set; }
        public string SerialKey { get; set; }
    }
}
