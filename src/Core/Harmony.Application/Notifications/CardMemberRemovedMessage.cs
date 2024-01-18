using Harmony.Application.DTO;

namespace Harmony.Application.Notifications
{
    public class CardMemberRemovedMessage
    {
        public CardMemberRemovedMessage(Guid boardId, Guid cardId, CardMemberDto member)
        {
            BoardId = boardId;
            CardId = cardId;
            Member = member;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public CardMemberDto Member { get; set; }
    }
}
