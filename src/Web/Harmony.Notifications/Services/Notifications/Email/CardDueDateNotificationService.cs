using Hangfire;
using Harmony.Persistence.DbContext;
using Harmony.Application.Helpers;
using Harmony.Domain.Enums;
using Harmony.Notifications.Contracts.Notifications.Email;
using Harmony.Application.Notifications.Email;
using Harmony.Application.Configurations;
using Microsoft.Extensions.Options;
using Grpc.Net.Client;
using Harmony.Api.Protos;
using Harmony.Domain.Entities;
using Harmony.Shared.Utilities;

namespace Harmony.Notifications.Services.Notifications.Email
{
    public class CardDueDateNotificationService : BaseNotificationService, ICardDueDateNotificationService
    {
        private readonly IEmailService _emailNotificationService;
        private readonly AppEndpointConfiguration _endpointConfiguration;

        public CardDueDateNotificationService(
            IEmailService emailNotificationService,
            NotificationContext notificationContext,
            IOptions<AppEndpointConfiguration> endpointsConfiguration) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _endpointConfiguration = endpointsConfiguration.Value;
        }

        public async Task Notify(CardDueTimeUpdatedNotification notification)
        {
            var cardId = notification.Id;

            await RemovePendingCardJobs(cardId, EmailNotificationType.CardDueDateUpdated);

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
                                  Board = false
                              });

            if (!cardResponse.Found)
            {
                return;
            }

            var card = cardResponse.Card;
            var dueDateIsNull = card.DueDate.Nanos == 0 && card.DueDate.Seconds == 0;
            
            if (dueDateIsNull || !card.DueDateReminderType.HasValue
                || card.DueDateReminderType.HasValue
                && (DueDateReminderType)card.DueDateReminderType == DueDateReminderType.None)
            {
                return;
            }

            var delay = card.DueDate.ToDateTime() - DateTime.Now;

            switch ((DueDateReminderType)card.DueDateReminderType)
            {
                case DueDateReminderType.FiveMinutesBefore:
                    delay = delay.Add(TimeSpan.FromMinutes(-5));
                    break;
                case DueDateReminderType.TenMinutesBefore:
                    delay = delay.Add(TimeSpan.FromMinutes(-10));
                    break;
                case DueDateReminderType.FifteenMinutesBefore:
                    delay = delay.Add(TimeSpan.FromMinutes(-15));
                    break;
                case DueDateReminderType.OneHourBefore:
                    delay = delay.Add(TimeSpan.FromHours(-1));
                    break;
                case DueDateReminderType.TwoHoursBefore:
                    delay = delay.Add(TimeSpan.FromHours(-2));
                    break;
                case DueDateReminderType.OneDayBefore:
                    delay = delay.Add(TimeSpan.FromDays(-1));
                    break;
                case DueDateReminderType.TwoDaysBefore:
                    delay = delay.Add(TimeSpan.FromDays(-2));
                    break;
                default:
                    break;
            }

            var jobId = BackgroundJob.Schedule(() => Notify(cardId), delay);

            if (string.IsNullOrEmpty(jobId))
            {
                return;
            }

            _notificationContext.Notifications.Add(new Notification()
            {
                CardId = cardId,
                JobId = jobId,
                Type = EmailNotificationType.CardDueDateUpdated,
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
                                  IssueType = true,
                                  Members = true
                              });

            var card = cardResponse.Card;
            var dueDateIsNull = card.DueDate.Nanos == 0 && card.DueDate.Seconds == 0;

            if (!cardResponse.Found || dueDateIsNull || card.IsCompleted || 
                (card.DueDateReminderType.HasValue && (DueDateReminderType)card.DueDateReminderType == DueDateReminderType.None))
            {
                return;
            }

            var reminderType = (DueDateReminderType)card.DueDateReminderType;
            var subject = $"{card.Title} in {card.BoardTitle} is due {GetDueMessage(reminderType)}";

            var userServiceClient = new UserService.UserServiceClient(channel);
            var usersFilterRequest = new UsersFilterRequest() { };

            usersFilterRequest.Users.AddRange(card.Members);

            var usersResponse = await userServiceClient.GetUsersAsync(usersFilterRequest);
            var cardMembers = usersResponse.Users;

            var userNotificationServiceClient = new UserNotificationService.UserNotificationServiceClient(channel);
            var userNotificationsFilterRequest = new GetUsersForNotificationTypeRequest() { };

            userNotificationsFilterRequest.Users.AddRange(card.Members);
            userNotificationsFilterRequest.Type = (int)EmailNotificationType.CardDueDateUpdated;

            var registeredUsersResponse = await userNotificationServiceClient.GetUsersForNotificationTypeAsync(userNotificationsFilterRequest);

            var startDateIsNull = card.StartDate.Nanos == 0 && card.StartDate.Seconds == 0;

            var slug = StringUtilities.SlugifyString(card.BoardTitle);
            var cardLink = $"{_endpointConfiguration.FrontendUrl}/boards/{card.BoardId}/{slug}/{card.CardId}";

            foreach (var member in cardMembers.Where(m => registeredUsersResponse.Users.Contains(m.Id)))
            {
                var content = EmailTemplates.EmailTemplates
                    .BuildFromGenericTemplate(_endpointConfiguration.FrontendUrl,
                    title: $"ISSUE IS DUE",
                    firstName: member.FirstName,
                    emailNotification: $"<strong>{card.Title}</strong> is due {GetDueMessage(reminderType)}.",
                    customerAction: $"<br/> <strong>Card due date</strong>: {CardHelper.DisplayDates(startDateIsNull ? null : card.StartDate.ToDateTime(), card.DueDate.ToDateTime())}",
                buttonText: "VIEW CARD",
                    buttonLink: cardLink);

                await _emailNotificationService.SendEmailAsync(member.Email, subject, content);
            }
        }

        private string GetDueMessage(DueDateReminderType type)
        {
            switch (type)
            {
                case DueDateReminderType.FiveMinutesBefore:
                    return "in 5 minutes";
                case DueDateReminderType.TenMinutesBefore:
                    return "in 10 minutes";
                case DueDateReminderType.FifteenMinutesBefore:
                    return "in 15 minutes";
                case DueDateReminderType.OneHourBefore:
                    return "in an hour";
                case DueDateReminderType.TwoHoursBefore:
                    return "in 2 hours";
                case DueDateReminderType.OneDayBefore:
                    return "in a day";
                case DueDateReminderType.TwoDaysBefore:
                    return "in 2 days";
                default:
                    return string.Empty;
            }
        }
    }
}
