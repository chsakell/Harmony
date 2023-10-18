using Harmony.Application.DTO;

namespace Harmony.Application.Events
{
    public class CardLabelToggledEvent
    {
        public Guid CardId { get; set; }
        public LabelDto Label { get; set; }
        public CardLabelToggledEvent(Guid cardId, LabelDto label)
        {
            CardId = cardId;
            Label = label;
        }
    }
}
