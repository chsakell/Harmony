using Harmony.Application.DTO;

namespace Harmony.Application.Events
{
    public class CardLabelToggledEvent
    {
        public int CardId { get; set; }
        public LabelDto Label { get; set; }
        public CardLabelToggledEvent(int cardId, LabelDto label)
        {
            CardId = cardId;
            Label = label;
        }
    }
}
