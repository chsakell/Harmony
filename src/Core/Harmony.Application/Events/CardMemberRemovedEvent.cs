using Harmony.Application.DTO;

namespace Harmony.Application.Events
{
    public class CardMemberRemovedEvent
    {
        public CardMemberRemovedEvent(Guid cardId, CardMemberDto member)
        {
            CardId = cardId;
            Member = member;
        }

        public Guid CardId { get; set; }
        public CardMemberDto Member { get; set; }
    }
}
