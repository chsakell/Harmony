using Hangfire;
using Harmony.Persistence.DbContext;
using Harmony.Domain.Enums;
using Harmony.Notifications.Contracts.Notifications.Email;
using Harmony.Application.Notifications.Email;
using Harmony.Application.Configurations;
using Microsoft.Extensions.Options;
using Grpc.Net.Client;
using Harmony.Api.Protos;
using Harmony.Domain.Entities;

namespace Harmony.Notifications.Services.Notifications.Email
{
    public class MemberRemovedFromCardNotificationService : BaseNotificationService, IMemberRemovedFromCardNotificationService
    {
        private readonly IEmailService _emailNotificationService;
        private readonly AppEndpointConfiguration _endpointConfiguration;

        public MemberRemovedFromCardNotificationService(
            IEmailService emailNotificationService,
            NotificationContext notificationContext,
            IOptions<AppEndpointConfiguration> endpointsConfiguration) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _endpointConfiguration = endpointsConfiguration.Value;
        }

        public async Task Notify(MemberRemovedFromCardNotification notification)
        {
            await RemovePendingCardJobs(notification.CardId, notification.UserId, EmailNotificationType.MemberRemovedFromCard);

            var jobId = BackgroundJob.Enqueue(() => SendEmail(notification));

            if (string.IsNullOrEmpty(jobId))
            {
                return;
            }

            _notificationContext.Notifications.Add(new Notification()
            {
                BoardId = notification.BoardId,
                JobId = jobId,
                Type = EmailNotificationType.MemberRemovedFromCard,
                DateCreated = DateTime.Now,
            });

            await _notificationContext.SaveChangesAsync();
        }

        public async Task SendEmail(MemberRemovedFromCardNotification notification)
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var channel = GrpcChannel.ForAddress(_endpointConfiguration.HarmonyApiEndpoint,
                new GrpcChannelOptions { HttpHandler = httpHandler });

            var client = new UserCardService.UserCardServiceClient(channel);

            var userCard = await client.GetUserCardAsync(new UserCardFilterRequest()
            {
                CardId = notification.CardId.ToString(),
                UserId = notification.UserId
            });

            if (userCard.Found)
            {
                return;
            }

            var boardServiceClient = new BoardService.BoardServiceClient(channel);

            var boardResponse = await boardServiceClient.GetBoardAsync(new BoardFilterRequest()
            {
                BoardId = notification.BoardId.ToString()
            });

            if (!boardResponse.Found)
            {
                return;
            }

            var board = boardResponse.Board;

            var cardServiceClient = new CardService.CardServiceClient(channel);
            var cardResponse = await cardServiceClient.GetCardAsync(
                              new CardFilterRequest
                              {
                                  CardId = notification.CardId.ToString()
                              });

            if (!cardResponse.Found)
            {
                return;
            }

            var card = cardResponse.Card;

            var userServiceClient = new UserService.UserServiceClient(channel);
            var userResponse = await userServiceClient.GetUserAsync(
                              new UserFilterRequest
                              {
                                  UserId = notification.UserId
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
                                  Type = (int)EmailNotificationType.MemberRemovedFromCard
                              });

            if (!userIsRegisteredResponse.IsRegistered)
            {
                return;
            }

            var subject = $"No logner assigned to {card.Title} in {board.Title}";

            var content = EmailTemplates.EmailTemplates
                    .BuildFromGenericTemplate(_endpointConfiguration.FrontendUrl,
                    title: $"ISSUE ASSIGNMENT REMOVED",
                    firstName: user.FirstName,
                    emailNotification: $"You are no longer assigned to <strong>{card.Title}</strong> on {board.Title}.",
                    customerAction: $"You can open the card by clicking the following link.",
                buttonText: "VIEW CARD",
                    buttonLink: notification.CardUrl);

            await _emailNotificationService.SendEmailAsync(user.Email, subject, content);
        }
    }
}
