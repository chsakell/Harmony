using Harmony.Application.DTO.Search;
using Harmony.Domain.Enums;
namespace Harmony.Application.Notifications.SearchIndex
{
    public class BoardCreatedIndexNotification : BaseSearchIndexNotification
    {
        public override SearchIndexNotificationType Type =>  SearchIndexNotificationType.BoardCreated;

        public string ObjectID { get; set; }  // Board Id
        public string Title { get; set; }
    }
}
