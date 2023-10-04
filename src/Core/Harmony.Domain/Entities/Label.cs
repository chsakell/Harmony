namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Class to represent a board's labels
    /// Can be referenced to board's cards
    /// </summary>
    public class Label : AuditableEntity<Guid>
    {
        public string Title { get; set; }
        public string Colour { get; set; }
        public Board Board { get; set; }
        public Guid BoardId { get; set; }
        public List<CardLabel> Labels { get; set; }
    }
}
