using Harmony.Application.DTO;

namespace Harmony.Application.Notifications
{
    public class CardLabelToggledMessage
    {
        public CardLabelToggledMessage(Guid boardId, Guid cardId, LabelDto label)
        {
            BoardId = boardId;
            CardId = cardId;
            Label = label;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public LabelDto Label { get; set; }
    }
}
