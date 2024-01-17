namespace Harmony.Application.Notifications
{
    public class CardDatesChangedMessage
    {
        public CardDatesChangedMessage(Guid boardId, Guid cardId, DateTime? startDate, DateTime? dueDate)
        {
            BoardId = boardId;
            CardId = cardId;
            StartDate = startDate;
            DueDate = dueDate;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
