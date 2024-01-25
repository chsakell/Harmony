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
using Harmony.Application.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Server.Kestrel;
using Grpc.Net.Client;
using Harmony.Api.Protos;

namespace Harmony.Notifications.Services.Notifications.Email
{
    public class MemberRemovedFromCardNotificationService : BaseNotificationService, IMemberRemovedFromCardNotificationService
    {
        private readonly IEmailService _emailNotificationService;
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly AppEndpointConfiguration _endpointConfiguration;

        public MemberRemovedFromCardNotificationService(
            IEmailService emailNotificationService,
            NotificationContext notificationContext,
            IUserNotificationRepository userNotificationRepository,
            IOptions<AppEndpointConfiguration> endpointsConfiguration) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _userNotificationRepository = userNotificationRepository;
            _endpointConfiguration = endpointsConfiguration.Value;
        }

        public async Task Notify(MemberRemovedFromCardNotification notification)
        {
            await RemovePendingCardJobs(notification.CardId, notification.UserId, EmailNotificationType.MemberRemovedFromCard);

            using var channel = GrpcChannel.ForAddress(_endpointConfiguration.HarmonyApiEndpoint);
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
            using var channel = GrpcChannel.ForAddress(_endpointConfiguration.HarmonyApiEndpoint);
            var boardServiceClient = new BoardService.BoardServiceClient(channel);

            var board = await boardServiceClient.GetBoardAsync(new BoardFilterRequest()
            {
                BoardId = notification.BoardId.ToString()
            });

            if (board == null)
            {
                return;
            }

            var cardServiceClient = new CardService.CardServiceClient(channel);
            var card = await cardServiceClient.GetCardAsync(
                              new CardFilterRequest
                              {
                                  CardId = notification.CardId.ToString()
                              });

            if (card == null)
            {
                return;
            }

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

            var notificationRegistration = await _userNotificationRepository
                .GetForUser(user.Id, EmailNotificationType.MemberRemovedFromCard);

            if (notificationRegistration == null)
            {
                return;
            }

            var subject = $"No logner assigned to {card.Title} in {board.Title}";

            var content = $"Dear {user.FirstName} {user.LastName},<br/><br/>" +
                $"You are no longer assigned to <a href='{notification.CardUrl}' target='_blank'>{card.Title}</a> on {board.Title}.";

            await _emailNotificationService.SendEmailAsync(user.Email, subject, content);
        }
    }
}
