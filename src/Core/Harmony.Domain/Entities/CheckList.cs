namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Class to represent a check list in a card
    /// </summary>
    public class CheckList : AuditableEntity<Guid>
    {
        public string Title { get; set; }
        public Card Card { get; set; }
        public Guid CardId { get; set; }
        public List<CheckListItem> Items { get; set; }
        public byte Position { get; set; }
    }
}
