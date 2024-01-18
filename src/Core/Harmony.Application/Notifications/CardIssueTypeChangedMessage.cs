using Harmony.Application.DTO;

namespace Harmony.Application.Notifications
{
    public class CardIssueTypeChangedMessage
    {
        public CardIssueTypeChangedMessage(Guid boardId, Guid cardId, IssueTypeDto issueType)
        {
            BoardId = boardId;
            CardId = cardId;
            IssueType = issueType;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public IssueTypeDto IssueType { get; set; }
    }
}
