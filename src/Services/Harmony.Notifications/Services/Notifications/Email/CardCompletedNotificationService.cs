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
        private readonly UserService.UserServiceClient _userServiceClient;
        private readonly CardService.CardServiceClient _cardServiceClient;
        private readonly UserNotificationService.UserNotificationServiceClient _userNotificationServiceClient;
        private readonly AppEndpointConfiguration _endpointConfiguration;

        public CardCompletedNotificationService(IEmailService emailNotificationService,
            NotificationContext notificationContext,
            IOptions<AppEndpointConfiguration> endpointsConfiguration,
            UserService.UserServiceClient userServiceClient,
            CardService.CardServiceClient cardServiceClient,
            UserNotificationService.UserNotificationServiceClient userNotificationServiceClient) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _userServiceClient = userServiceClient;
            _cardServiceClient = cardServiceClient;
            _userNotificationServiceClient = userNotificationServiceClient;
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

            _notificationContext.Tasks.Add(new Notification()
            {
                CardId = cardId,
                JobId = jobId,
                Type = (int)EmailNotificationType.CardCompleted,
                DateCreated = DateTime.Now,
            });

            await _notificationContext.SaveChangesAsync();
        }

        public async Task Notify(Guid cardId)
        {           
            var cardResponse = await _cardServiceClient.GetCardAsync(
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

            var usersFilterRequest = new UsersFilterRequest() {};

            usersFilterRequest.Users.AddRange(card.Members);

            var usersResponse = await _userServiceClient.GetUsersAsync(usersFilterRequest);
            var cardMembers = usersResponse.Users;

            var userNotificationsFilterRequest = new GetUsersForNotificationTypeRequest() { };

            userNotificationsFilterRequest.Users.AddRange(card.Members);
            userNotificationsFilterRequest.Type = (int)EmailNotificationType.CardCompleted;

            var registeredUsersResponse = await _userNotificationServiceClient.GetUsersForNotificationTypeAsync(userNotificationsFilterRequest);

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
