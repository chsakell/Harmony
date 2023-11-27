namespace Harmony.Application.Events
{
    public class CardLabelRemovedEvent
    {
        public Guid CardLabelId { get; set; }

        public CardLabelRemovedEvent(Guid cardLabelId)
        {
            CardLabelId = cardLabelId;
        }
    }
}
