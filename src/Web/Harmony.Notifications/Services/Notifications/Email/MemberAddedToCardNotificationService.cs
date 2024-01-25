using Hangfire;
using Harmony.Application.Contracts.Repositories;
using Harmony.Notifications.Persistence;
using Harmony.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Specifications.Boards;
using Harmony.Domain.Enums;
using Harmony.Notifications.Contracts.Notifications.Email;
using Harmony.Application.Notifications.Email;
using Grpc.Net.Client;
using Harmony.Api.Protos;
using Harmony.Application.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Server.Kestrel;

namespace Harmony.Notifications.Services.Notifications.Email
{
    public class MemberAddedToCardNotificationService : BaseNotificationService, IMemberAddedToCardNotificationService
    {
        private readonly IEmailService _emailNotificationService;
        private readonly IUserService _userService;
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly AppEndpointConfiguration _endpointConfiguration;

        public MemberAddedToCardNotificationService(
            IEmailService emailNotificationService,
            IUserService userService,
            NotificationContext notificationContext,
            IUserNotificationRepository userNotificationRepository,
            IOptions<AppEndpointConfiguration> endpointsConfiguration) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _userService = userService;
            _userNotificationRepository = userNotificationRepository;
            _endpointConfiguration = endpointsConfiguration.Value;
        }

        public async Task Notify(MemberAddedToCardNotification notification)
        {
            await RemovePendingCardJobs(notification.CardId, notification.UserId, EmailNotificationType.MemberAddedToCard);

            using var channel = GrpcChannel.ForAddress(_endpointConfiguration.HarmonyApiEndpoint);
            var client = new UserCardService.UserCardServiceClient(channel);

            var userCard = await client.GetUserCardAsync(new UserCardFilterRequest()
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

            _notificationContext.Notifications.Add(new Notification()
            {
                BoardId = notification.BoardId,
                JobId = jobId,
                Type = EmailNotificationType.MemberAddedToCard,
                DateCreated = DateTime.Now,
            });

            await _notificationContext.SaveChangesAsync();
        }

        public async Task SendEmail(MemberAddedToCardNotification notification)
        {
            using var channel = GrpcChannel.ForAddress(_endpointConfiguration.HarmonyApiEndpoint);
            var boardServiceClient = new BoardService.BoardServiceClient(channel);

            var filter = new BoardFilterSpecification(notification.BoardId, new BoardIncludes());

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

            var userResult = await _userService.GetAsync(notification.UserId);

            if (!userResult.Succeeded || !userResult.Data.IsActive)
            {
                return;
            }
            var user = userResult.Data;

            var notificationRegistration = await _userNotificationRepository
                .GetForUser(user.Id, EmailNotificationType.MemberAddedToCard);

            if (notificationRegistration == null)
            {
                return;
            }

            var subject = $"Assigned to {card.Title} in {board.Title}";

            var content = $"Dear {user.FirstName} {user.LastName},<br/><br/>" +
                $"You have been assigned to <a href='{notification.CardUrl}' target='_blank'>{card.Title}</a> on {board.Title}.";

            await _emailNotificationService.SendEmailAsync(user.Email, subject, content);
        }
    }
}
