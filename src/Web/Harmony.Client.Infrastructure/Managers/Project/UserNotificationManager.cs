
using Harmony.Application.Features.Users.Commands.UpdateUserNotifications;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;
using static Harmony.Shared.Constants.Permission.Permissions;

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
                .GetAsync(Routes.UserNotificationsEndpoints.User(userId));

            return await response.ToResult<List<NotificationType>>();
        }

        public async Task<IResult<bool>> SetNotificationsAsync(List<NotificationType> notifications)
        {
            var response = await _httpClient
                .PostAsJsonAsync(Routes.UserNotificationsEndpoints.Index, 
                new UpdateUserNotificationsCommand(notifications));

            return await response.ToResult<bool>();
        }
    }
}
