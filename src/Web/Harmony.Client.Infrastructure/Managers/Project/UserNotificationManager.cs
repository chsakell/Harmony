
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    /// <summary>
    /// Client manager for user notifications
    /// </summary>
    public class UserNotificationManager : IUserNotificationManager
    {
        private readonly HttpClient _httpClient;

        public UserNotificationManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<List<NotificationType>>> GetNotificationsAsync(string userId)
        {
            var response = await _httpClient
                .GetAsync(Routes.UserNotificationsEndpoints.GetAllForUser(userId));

            return await response.ToResult<List<NotificationType>>();
        }
    }
}
