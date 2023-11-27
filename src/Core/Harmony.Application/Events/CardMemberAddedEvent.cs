using Harmony.Application.DTO;

namespace Harmony.Application.Events
{
    public class CardMemberAddedEvent
    {
        public CardMemberAddedEvent(Guid cardId, CardMemberDto member)
        {
            CardId = cardId;
            Member = member;
        }

        public Guid CardId { get; set; }
        public CardMemberDto Member { get; set; }
    }
}
