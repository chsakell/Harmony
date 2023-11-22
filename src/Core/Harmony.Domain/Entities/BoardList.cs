using Harmony.Domain.Enums;

namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Class to represent a board's lists/columns
    /// </summary>
    public class BoardList : AuditableEntity<Guid>
    {
        public string Title { get; set; }
        public Board Board { get; set; }
		public string UserId { get; set; } // User created the list
		public Guid BoardId { get; set; }
        public short Position { get; set; } // position on the board
        public List<Card> Cards { get; set; }
        public BoardListStatus Status { get; set; }
        public BoardListCardStatus? CardStatus { get; set; }
    }
}
