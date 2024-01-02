using Harmony.Domain.Enums;
namespace Harmony.Application.Notifications.SearchIndex
{
    public class CardStatusUpdatedIndexNotification : BaseSearchIndexNotification
    {
        public override SearchIndexNotificationType Type =>  SearchIndexNotificationType.CardStatusUpdated;

        public string ObjectID { get; set; }  // Card Id
        public string Status { get; set; }
    }
}
