using Harmony.Domain.Enums;

namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Represents a card's activity
    /// </summary>
    public class CardActivity : AuditableEntity<Guid>
    {
        public string Activity { get; set; }
        public Card Card { get; set; }
        public Guid CardId { get; set; }
        public string UserId { get; set; }
        public CardActivityType Type { get; set; }
        public string Url { get; set; }
    }
}
