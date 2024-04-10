using Harmony.Application.DTO;

namespace Harmony.Application.Notifications
{
    public class CardLinkDeletedMessage 
    {
        public Guid LinkId { get; set; }
        public Guid BoardId { get; set; }

        public CardLinkDeletedMessage(Guid linkId, Guid boardId)
        {
            LinkId = linkId;
            BoardId = boardId;
        }
    }
}
