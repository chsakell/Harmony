namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Class to represent a card's comment
    /// </summary>
    public class Comment : AuditableEntity<Guid>
    {
        public string Text { get; set; }
        public Card Card { get; set; }
        public Guid CardId { get; set; }
        public string UserId { get; set; }
    }
}
