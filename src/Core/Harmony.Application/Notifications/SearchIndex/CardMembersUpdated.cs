using Harmony.Domain.Enums;
namespace Harmony.Application.Notifications.SearchIndex
{
    public class CardMembersUpdatedIndexNotification : BaseSearchIndexNotification
    {
        public override SearchIndexNotificationType Type =>  SearchIndexNotificationType.CardMembersUpdated;

        public string ObjectID { get; set; }  // Card Id
        public List<string> Members { get; set; }
    }
}
