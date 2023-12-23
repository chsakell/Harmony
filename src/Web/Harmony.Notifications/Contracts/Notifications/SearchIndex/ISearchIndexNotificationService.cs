using Harmony.Application.DTO.Search;
using Harmony.Application.Notifications.SearchIndex;

namespace Harmony.Notifications.Contracts.Notifications.SearchIndex
{
    public interface ISearchIndexNotificationService
    {
        bool CreateIndex(string name);
        Task AddToIndex<T>(T notification) where T : class, ISearchIndexNotification;
        Task UpdateCard<T>(T notification) where T : class, ISearchIndexNotification;
    }
}
