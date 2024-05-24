using Hangfire;
using Harmony.Persistence.DbContext;
using Harmony.Application.Specifications.Boards;
using Harmony.Domain.Enums;
using Harmony.Notifications.Contracts.Notifications.Email;
using Harmony.Application.Notifications.Email;
using Harmony.Api.Protos;
using Harmony.Application.Configurations;
using Microsoft.Extensions.Options;
using Harmony.Domain.Entities;

namespace Harmony.Notifications.Services.Notifications.Email
{
    public class MemberAddedToCardNotificationService : BaseNotificationService, IMemberAddedToCardNotificationService
    {
        private readonly IEmailService _emailNotificationService;
        private readonly UserCardService.UserCardServiceClient _userCardServiceClient;
        private readonly BoardService.BoardServiceClient _boardServiceClient;
        private readonly CardService.CardServiceClient _cardServiceClient;
        private readonly UserService.UserServiceClient _userServiceClient;
        private readonly UserNotificationService.UserNotificationServiceClient _userNotificationServiceClient;
        private readonly AppEndpointConfiguration _endpointConfiguration;

        public MemberAddedToCardNotificationService(
            IEmailService emailNotificationService,
            NotificationContext notificationContext,
            UserCardService.UserCardServiceClient userCardServiceClient,
            BoardService.BoardServiceClient boardServiceClient,
            CardService.CardServiceClient cardServiceClient,
            UserService.UserServiceClient userServiceClient,
            UserNotificationService.UserNotificationServiceClient userNotificationServiceClient,
            IOptions<AppEndpointConfiguration> endpointsConfiguration) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _userCardServiceClient = userCardServiceClient;
            _boardServiceClient = boardServiceClient;
            _cardServiceClient = cardServiceClient;
            _userServiceClient = userServiceClient;
            _userNotificationServiceClient = userNotificationServiceClient;
            _endpointConfiguration = endpointsConfiguration.Value;
        }

        public async Task Notify(MemberAddedToCardNotification notification)
        {
            await RemovePendingCardJobs(notification.CardId, notification.UserId, EmailNotificationType.MemberAddedToCard);

            var userCard = await _userCardServiceClient.GetUserCardAsync(new UserCardFilterRequest()
            {
                CardId = notification.CardId.ToString(),
                UserId = notification.UserId
            });

            if (!userCard.Found)
            {
                return;
            }

            var jobId = BackgroundJob.Enqueue(() => SendEmail(notification));

            if (string.IsNullOrEmpty(jobId))
            {
                return;
            }

            _notificationContext.Tasks.Add(new Notification()
            {
                BoardId = notification.BoardId,
                JobId = jobId,
                Type = (int)EmailNotificationType.MemberAddedToCard,
                DateCreated = DateTime.Now,
            });

            await _notificationContext.SaveChangesAsync();
        }

        public async Task SendEmail(MemberAddedToCardNotification notification)
        {
            var filter = new BoardFilterSpecification()
            {
                BoardId = notification.BoardId
            };

            filter.Build();

            var boardResponse = await _boardServiceClient.GetBoardAsync(new BoardFilterRequest()
            {
                BoardId = notification.BoardId.ToString()
            });

            if (!boardResponse.Found)
            {
                return;
            }

            var board = boardResponse.Board;

            var cardResponse = await _cardServiceClient.GetCardAsync(
                              new CardFilterRequest
                              {
                                  CardId = notification.CardId.ToString()
                              });

            if (!cardResponse.Found)
            {
                return;
            }

            var card = cardResponse.Card;

            var userResponse = await _userServiceClient.GetUserAsync(
            new UserFilterRequest
            {
                UserId = notification.UserId
            });

            if (!userResponse.Found)
            {
                return;
            }

            var user = userResponse.User;

            var userIsRegisteredResponse = await _userNotificationServiceClient.UserIsRegisterForNotificationAsync(
                              new UserIsRegisterForNotificationRequest()
                              {
                                  UserId = notification.UserId,
                                  Type = (int)EmailNotificationType.MemberAddedToCard
                              });

            if (!userIsRegisteredResponse.IsRegistered)
            {
                return;
            }

            var subject = $"Assigned to {card.Title} in {board.Title}";

            var content = EmailTemplates.EmailTemplates
                    .BuildFromGenericTemplate(_endpointConfiguration.FrontendUrl,
                    title: $"ISSUE ASSIGNMENT",
                    firstName: user.FirstName,
                    emailNotification: $"You have been assigned to <strong>{card.Title}</strong> on {board.Title}.",
                    customerAction: $"You can open the card by clicking the following link.",
                buttonText: "VIEW CARD",
                    buttonLink: notification.CardUrl);

            await _emailNotificationService.SendEmailAsync(user.Email, subject, content);
        }
    }
}
