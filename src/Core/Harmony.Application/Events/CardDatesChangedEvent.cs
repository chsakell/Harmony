namespace Harmony.Application.Events
{
    public class CardDatesChangedEvent
    {
        public CardDatesChangedEvent(int cardId, DateTime? startDate, DateTime? dueDate)
        {
            CardId = cardId;
            StartDate = startDate;
            DueDate = dueDate;
        }

        public int CardId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
