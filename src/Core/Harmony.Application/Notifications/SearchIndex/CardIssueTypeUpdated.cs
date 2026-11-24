using Harmony.Domain.Enums;
namespace Harmony.Application.Notifications.SearchIndex
{
    public class CardIssueTypeUpdatedIndexNotification : BaseSearchIndexNotification
    {
        public override SearchIndexNotificationType Type =>  SearchIndexNotificationType.CardIssueTypeUpdated;

        public override string ObjectID { get; set; }  // Card Id
        public string IssueType { get; set; }
    }
}
