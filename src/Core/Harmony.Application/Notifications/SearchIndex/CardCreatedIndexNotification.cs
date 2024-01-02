using Harmony.Domain.Enums;
namespace Harmony.Application.Notifications.SearchIndex
{
    public class CardCreatedIndexNotification : BaseSearchIndexNotification
    {
        public override SearchIndexNotificationType Type =>  SearchIndexNotificationType.CardAddedToBoard;

        public string ObjectID { get; set; }  // Card Id
        public Guid BoardId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string ListId {  get; set; }
        public string IssueType { get; set; }
        public string SerialKey { get; set; }
    }
}
