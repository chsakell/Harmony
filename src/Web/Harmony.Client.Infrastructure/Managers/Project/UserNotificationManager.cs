
using Harmony.Application.Features.Users.Commands.UpdateUserNotifications;
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

        public async Task<IResult<List<EmailNotificationType>>> GetNotificationsAsync(string userId)
        {
            var response = await _httpClient
                .GetAsync(Routes.UserNotificationsEndpoints.User(userId));

            return await response.ToResult<List<EmailNotificationType>>();
        }

        public async Task<IResult<bool>> SetNotificationsAsync(List<EmailNotificationType> notifications)
        {
            var response = await _httpClient
                .PostAsJsonAsync(Routes.UserNotificationsEndpoints.Index, 
                new UpdateUserNotificationsCommand(notifications));

            return await response.ToResult<bool>();
        }
    }
}
