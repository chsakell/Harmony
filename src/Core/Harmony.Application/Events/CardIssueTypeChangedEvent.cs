using Harmony.Application.DTO;

namespace Harmony.Application.Events
{
    public class CardIssueTypeChangedEvent
    {
        public CardIssueTypeChangedEvent(Guid cardId, IssueTypeDto issueType)
        {
            CardId = cardId;
            IssueType = issueType;
        }

        public Guid CardId { get; set; }
        public IssueTypeDto IssueType { get; set; }

    }
}
