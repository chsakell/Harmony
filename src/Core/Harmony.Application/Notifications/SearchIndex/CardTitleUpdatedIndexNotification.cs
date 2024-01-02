using Harmony.Domain.Enums;
namespace Harmony.Application.Notifications.SearchIndex
{
    public class CardTitleUpdatedIndexNotification : BaseSearchIndexNotification
    {
        public override SearchIndexNotificationType Type =>  SearchIndexNotificationType.CardTitleUpdated;

        public string ObjectID { get; set; }  // Card Id
        public string Title { get; set; }
    }
}
