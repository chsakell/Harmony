using Hangfire;
using Harmony.Persistence.DbContext;
using Harmony.Domain.Enums;
using Harmony.Notifications.Contracts.Notifications.Email;
using Harmony.Application.Notifications.Email;
using Harmony.Api.Protos;
using Grpc.Net.Client;
using Harmony.Application.Configurations;
using Microsoft.Extensions.Options;
using Harmony.Domain.Entities;

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
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var channel = GrpcChannel.ForAddress(_endpointConfiguration.HarmonyApiEndpoint,
                new GrpcChannelOptions { HttpHandler = httpHandler });

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

            var subject = $"Access {board.Title} in {board.Workspace.Name} revoked";

            var content = EmailTemplates.EmailTemplates
                    .BuildFromNoActionGenericTemplate(_endpointConfiguration.FrontendUrl,
                    title: $"ACCESS TO BOARD GRANTED",
                    firstName: user.FirstName,
                    emailNotification: $"Accesss to <strong>{board.Title}</strong> has been revoked.",
                    customerAction: $"You have no longer permission to access {board.Title}.");

            await _emailNotificationService.SendEmailAsync(user.Email, subject, content);
        }
    }
}
