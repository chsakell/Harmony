using Harmony.Domain.Enums;
namespace Harmony.Application.Notifications.SearchIndex
{
    public class CardDueDateUpdatedIndexNotification : BaseSearchIndexNotification
    {
        public override SearchIndexNotificationType Type =>  SearchIndexNotificationType.CardDueDateUpdated;

        public string ObjectID { get; set; }  // Card Id
        public long? DueDate { get; set; }
    }
}
