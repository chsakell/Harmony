using Harmony.Domain.Enums;
namespace Harmony.Application.Notifications.SearchIndex
{
    public class CardHasAttachmentsUpdatedIndexNotification : BaseSearchIndexNotification
    {
        public override SearchIndexNotificationType Type =>  SearchIndexNotificationType.CardHasAttachmentsUpdated;

        public string ObjectID { get; set; }  // Card Id
        public bool HasAttachments { get; set; }
    }
}
