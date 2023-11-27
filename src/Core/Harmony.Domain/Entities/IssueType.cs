namespace Harmony.Domain.Entities
{
    public class IssueType : AuditableEntity<Guid>
    {
        public string Summary { get; set; }
        public string Description { get; set; }
        public Board Board { get; set; }
        public Guid BoardId { get; set; }
        public List<Card> Cards { get; set; }
    }
}
