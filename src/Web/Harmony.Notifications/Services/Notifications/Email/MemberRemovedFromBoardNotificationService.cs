using Hangfire;
using Harmony.Notifications.Persistence;
using Harmony.Domain.Enums;
using Harmony.Notifications.Contracts.Notifications.Email;
using Harmony.Application.Notifications.Email;
using Harmony.Api.Protos;
using Grpc.Net.Client;
using Harmony.Application.Configurations;
using Microsoft.Extensions.Options;

namespace Harmony.Notifications.Services.Notifications.Email
{
    public class MemberRemovedFromBoardNotificationService : BaseNotificationService, IMemberRemovedFromBoardNotificationService
    {
        private readonly IEmailService _emailNotificationService;
        private readonly AppEndpointConfiguration _endpointConfiguration;

        public MemberRemovedFromBoardNotificationService(
            IEmailService emailNotificationService,
            NotificationContext notificationContext,
            IOptions<AppEndpointConfiguration> endpointsConfiguration) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _endpointConfiguration = endpointsConfiguration.Value;
        }

        public async Task Notify(MemberRemovedFromBoardNotification notification)
        {
            await RemovePendingBoardJobs(notification.BoardId, notification.UserId, EmailNotificationType.MemberRemovedFromBoard);

            var jobId = BackgroundJob.Enqueue(() => Notify(notification.BoardId, notification.UserId, notification.BoardUrl));

            if (string.IsNullOrEmpty(jobId))
            {
                return;
            }

            _notificationContext.Notifications.Add(new Notification()
            {
                BoardId = notification.BoardId,
                JobId = jobId,
                Type = EmailNotificationType.MemberRemovedFromBoard,
                DateCreated = DateTime.Now,
            });

            await _notificationContext.SaveChangesAsync();
        }


        public async Task Notify(Guid boardId, string userId, string boardUrl)
        {
            using var channel = GrpcChannel.ForAddress(_endpointConfiguration.HarmonyApiEndpoint);
            var boardServiceClient = new BoardService.BoardServiceClient(channel);

            var boardResponse = await boardServiceClient.GetBoardAsync(new BoardFilterRequest()
            {
                BoardId = boardId.ToString(),
                Workspace = true
            });

            if (!boardResponse.Found)
            {
                return;
            }

            var board = boardResponse.Board;

            var userServiceClient = new UserService.UserServiceClient(channel);
            var userResponse = await userServiceClient.GetUserAsync(
                              new UserFilterRequest
                              {
                                  UserId = userId
                              });

            if (!userResponse.Found)
            {
                return;
            }
            var user = userResponse.User;

            var userNotificationServiceClient = new UserNotificationService.UserNotificationServiceClient(channel);
            var userIsRegisteredResponse = await userNotificationServiceClient.UserIsRegisterForNotificationAsync(
                              new UserIsRegisterForNotificationRequest()
                              {
                                  UserId = userId,
                                  Type = (int)EmailNotificationType.MemberRemovedFromBoard
                              });

            if (!userIsRegisteredResponse.IsRegistered)
            {
                return;
            }

            var subject = $"Access {board.Title} in {board.Workspace.Name} is revoked";

            var content = $"Dear {user.FirstName} {user.LastName},<br/><br/>" +
                $"Your access to <strong>{board.Title}</strong> on {board.Workspace.Name} workspace is now revoked.";

            await _emailNotificationService.SendEmailAsync(user.Email, subject, content);
        }
    }
}
