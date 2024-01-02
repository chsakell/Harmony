using Harmony.Domain.Enums;
namespace Harmony.Application.Notifications.SearchIndex
{
    public class CardDescriptionUpdatedIndexNotification : BaseSearchIndexNotification
    {
        public override SearchIndexNotificationType Type =>  SearchIndexNotificationType.CardDescriptionUpdated;

        public string ObjectID { get; set; }  // Card Id
        public string Description { get; set; }
    }
}
