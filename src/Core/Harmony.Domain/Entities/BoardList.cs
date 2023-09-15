namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Class to represent a board's lists/columns
    /// </summary>
    public class BoardList : AuditableEntity<Guid>
    {
        public string Name { get; set; }
        public Board Board { get; set; }
        public Guid BoardId { get; set; }
        public byte Position { get; set; } // position on the board
        public List<Card> Cards { get; set; }
    }
}
