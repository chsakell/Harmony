using Harmony.Application.DTO.Search;
using Harmony.Domain.Enums;
namespace Harmony.Application.Notifications.SearchIndex
{
    public class CardAddedIndexNotification : BaseSearchIndexNotification
    {
        public override SearchIndexNotificationType Type =>  SearchIndexNotificationType.CardAddedToBoard;

        public Guid BoardId { get; set; }
        public string ObjectID { get; set; }  // Card Id
        public string Title { get; set; }
        public string Status { get; set; }
        public string ListId {  get; set; }
        public string IssueType { get; set; }
    }
}
