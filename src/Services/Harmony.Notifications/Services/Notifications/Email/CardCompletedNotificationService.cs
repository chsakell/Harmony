using Hangfire;
using Harmony.Persistence.DbContext;
using Harmony.Domain.Enums;
using Harmony.Notifications.Contracts.Notifications.Email;
using Harmony.Application.Notifications.Email;
using Harmony.Application.Configurations;
using Microsoft.Extensions.Options;
using Harmony.Api.Protos;
using Grpc.Net.Client;
using Harmony.Shared.Utilities;
using Harmony.Domain.Entities;

namespace Harmony.Notifications.Services.Notifications.Email
{
    public class CardCompletedNotificationService : BaseNotificationService, ICardCompletedNotificationService
    {
        private readonly IEmailService _emailNotificationService;
        private readonly AppEndpointConfiguration _endpointConfiguration;

        public CardCompletedNotificationService(IEmailService emailNotificationService,
            NotificationContext notificationContext,
            IOptions<AppEndpointConfiguration> endpointsConfiguration) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _endpointConfiguration = endpointsConfiguration.Value;
        }

        public async Task Notify(CardCompletedNotification notification)
        {
            var cardId = notification.Id;

            await RemovePendingCardJobs(cardId, EmailNotificationType.CardCompleted);

            var jobId = BackgroundJob.Schedule(() => Notify(cardId), TimeSpan.FromSeconds(10));

            if (string.IsNullOrEmpty(jobId))
            {
                return;
            }

            _notificationContext.Notifications.Add(new Notification()
            {
                CardId = cardId,
                JobId = jobId,
                Type = EmailNotificationType.CardCompleted,
                DateCreated = DateTime.Now,
            });

            await _notificationContext.SaveChangesAsync();
        }

        public async Task Notify(Guid cardId)
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var channel = GrpcChannel.ForAddress(_endpointConfiguration.HarmonyApiEndpoint,
                new GrpcChannelOptions { HttpHandler = httpHandler });
            var cardServiceClient = new CardService.CardServiceClient(channel);
            var cardResponse = await cardServiceClient.GetCardAsync(
                              new CardFilterRequest
                              {
                                  CardId = cardId.ToString(),
                                  Board = true,
                                  Members = true
                              });

            if (!cardResponse.Found || !cardResponse.Card.IsCompleted)
            {
                return;
            }

            var card = cardResponse.Card;

            var subject = $"{card.Title} in {card.BoardTitle} completed";

            var userServiceClient = new UserService.UserServiceClient(channel);
            var usersFilterRequest = new UsersFilterRequest() {};

            usersFilterRequest.Users.AddRange(card.Members);

            var usersResponse = await userServiceClient.GetUsersAsync(usersFilterRequest);
            var cardMembers = usersResponse.Users;

            var userNotificationServiceClient = new UserNotificationService.UserNotificationServiceClient(channel);
            var userNotificationsFilterRequest = new GetUsersForNotificationTypeRequest() { };

            userNotificationsFilterRequest.Users.AddRange(card.Members);
            userNotificationsFilterRequest.Type = (int)EmailNotificationType.CardCompleted;

            var registeredUsersResponse = await userNotificationServiceClient.GetUsersForNotificationTypeAsync(userNotificationsFilterRequest);

            var slug = StringUtilities.SlugifyString(card.BoardTitle);

            var boardLink = $"{_endpointConfiguration.FrontendUrl}/boards/{card.BoardId}/{slug}";

            foreach (var member in cardMembers.Where(m => registeredUsersResponse.Users.Contains(m.Id)))
            {
                var content = EmailTemplates.EmailTemplates
                    .BuildFromGenericTemplate(_endpointConfiguration.FrontendUrl,
                    title: $"ISSUE COMPLETED",
                    firstName: member.FirstName,
                    emailNotification: $"<strong>{card.Title}</strong> has been completed.",
                    customerAction: "You can continue working on the board",
                    buttonText: "VIEW BOARD",
                    buttonLink: boardLink);

                await _emailNotificationService.SendEmailAsync(member.Email, subject, content);
            }
        }
    }
}
