using Harmony.Domain.Enums;
namespace Harmony.Application.Notifications.SearchIndex
{
    public class CardListUpdatedIndexNotification : BaseSearchIndexNotification
    {
        public override SearchIndexNotificationType Type =>  SearchIndexNotificationType.CardListUpdated;

        public override string ObjectID { get; set; }  // Card Id
        public string ListId { get; set; }
    }
}
