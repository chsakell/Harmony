using Harmony.Application.DTO.Search;
using Harmony.Application.Notifications.SearchIndex;

namespace Harmony.Notifications.Contracts.Notifications.SearchIndex
{
    public interface ISearchIndexNotificationService
    {
        bool CreateIndex(string name);
        Task AddCardToIndex(CardAddedIndexNotification notification);
        Task UpdateCard(Guid boardId, SearchableCard card);
    }
}
