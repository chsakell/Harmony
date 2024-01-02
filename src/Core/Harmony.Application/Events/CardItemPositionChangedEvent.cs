namespace Harmony.Application.Events
{
    public class CardItemPositionChangedEvent
    {
        public CardItemPositionChangedEvent(Guid boardId, Guid cardId, Guid previousBoardListId, Guid newBoardListId, short previousPosition, short newPosition, Guid updateId)
        {
            BoardId = boardId;
            CardId = cardId;
            PreviousBoardListId = previousBoardListId;
            NewBoardListId = newBoardListId;
            PreviousPosition = previousPosition;
            NewPosition = newPosition;
            UpdateId = updateId;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public Guid PreviousBoardListId { get; set; }
        public Guid NewBoardListId { get; set; }
        public short PreviousPosition { get; set; }
        public short NewPosition { get; set; }
        public Guid UpdateId { get; set; }
    }
}
