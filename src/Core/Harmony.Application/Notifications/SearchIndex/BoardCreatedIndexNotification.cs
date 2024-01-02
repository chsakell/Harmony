using Harmony.Domain.Enums;
namespace Harmony.Application.Notifications.SearchIndex
{
    public class BoardCreatedIndexNotification : BaseSearchIndexNotification
    {
        public override SearchIndexNotificationType Type =>  SearchIndexNotificationType.BoardCreated;

        public string ObjectID { get; set; }  // Board Id
        public List<string> SearchableAttributes { get; set; }
        public List<string> AttributesForFaceting { get; set; }
    }
}
