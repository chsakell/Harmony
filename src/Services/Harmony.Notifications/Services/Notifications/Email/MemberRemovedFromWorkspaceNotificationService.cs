using Hangfire;
using Harmony.Persistence.DbContext;
using Harmony.Domain.Enums;
using Harmony.Notifications.Contracts.Notifications.Email;
using Harmony.Application.Notifications.Email;
using Grpc.Net.Client;
using Harmony.Api.Protos;
using Harmony.Application.Configurations;
using Microsoft.Extensions.Options;
using Harmony.Domain.Entities;

namespace Harmony.Notifications.Services.Notifications.Email
{
    public class MemberRemovedFromWorkspaceNotificationService : BaseNotificationService, IMemberRemovedFromWorkspaceNotificationService
    {
        private readonly IEmailService _emailNotificationService;
        private readonly AppEndpointConfiguration _endpointConfiguration;

        public MemberRemovedFromWorkspaceNotificationService(
            IEmailService emailNotificationService,
            NotificationContext notificationContext,
            IOptions<AppEndpointConfiguration> endpointsConfiguration) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _endpointConfiguration = endpointsConfiguration.Value;
        }

        public async Task Notify(MemberRemovedFromWorkspaceNotification notification)
        {
            await RemovePendingWorkspaceJobs(notification.WorkspaceId, notification.UserId, EmailNotificationType.MemberRemovedFromWorkspace);

            var jobId = BackgroundJob.Enqueue(() =>
                Notify(notification.WorkspaceId, notification.UserId, notification.WorkspaceUrl));

            if (string.IsNullOrEmpty(jobId))
            {
                return;
            }

            _notificationContext.Notifications.Add(new Notification()
            {
                WorkspaceId = notification.WorkspaceId,
                JobId = jobId,
                Type = EmailNotificationType.MemberRemovedFromWorkspace,
                DateCreated = DateTime.Now,
            });

            await _notificationContext.SaveChangesAsync();
        }


        public async Task Notify(Guid workspaceId, string userId, string workspaceUrl)
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var channel = GrpcChannel.ForAddress(_endpointConfiguration.HarmonyApiEndpoint,
                new GrpcChannelOptions { HttpHandler = httpHandler });

            var workspaceServiceClient = new WorkspaceService.WorkspaceServiceClient(channel);
            var workspaceResponse = await workspaceServiceClient.GetWorkspaceAsync(new WorkspaceFilterRequest
            {
                WorkspaceId = workspaceId.ToString()
            });

            if (!workspaceResponse.Found)
            {
                return;
            }

            var workspace = workspaceResponse.Workspace;

            var userServiceClient = new UserService.UserServiceClient(channel);
            var userResponse = await userServiceClient.GetUserAsync(new UserFilterRequest
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
                                  UserId = user.Id,
                                  Type = (int)EmailNotificationType.MemberRemovedFromWorkspace
                              });

            if (!userIsRegisteredResponse.IsRegistered)
            {
                return;
            }

            var subject = $"Access to {workspace.Name} workspace revoked";

            var content = EmailTemplates.EmailTemplates
                    .BuildFromNoActionGenericTemplate(_endpointConfiguration.FrontendUrl,
                    title: $"WORKSPACE PERMISSION REVOKED",
                    firstName: user.FirstName,
                    emailNotification: $"Your access to <strong>{workspace.Name}</strong> has been revoked.",
                    customerAction: $"Contact the administrator if this happened for no reason.");

            await _emailNotificationService.SendEmailAsync(user.Email, subject, content);
        }
    }
}
