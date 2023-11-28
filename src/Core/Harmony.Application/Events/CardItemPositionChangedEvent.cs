namespace Harmony.Application.Events
{
    public class CardItemPositionChangedEvent
    {
        public CardItemPositionChangedEvent(Guid boardId, Guid boardListId, Guid cardId, short position)
        {
            BoardId = boardId;
            BoardListId = boardListId;
            CardId = cardId;
            Position = position;
        }

        public Guid BoardId { get; set; }
        public Guid BoardListId { get; set; }
        public Guid CardId { get; set; }
        public short Position { get; set; }
    }
}
