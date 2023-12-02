using Hangfire;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Specifications.Cards;
using Harmony.Notifications.Contracts;
using Harmony.Notifications.Persistence;
using Harmony.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Enums;
using Harmony.Application.Helpers;
using Harmony.Domain.Enums;

namespace Harmony.Notifications.Services
{
    public class CardDueDateNotificationService : BaseNotificationService, ICardDueDateNotificationService
    {
        private readonly IEmailService _emailNotificationService;
        private readonly IUserService _userService;
        private readonly ICardRepository _cardRepository;

        public CardDueDateNotificationService(IEmailService emailNotificationService,
            IUserService userService,
            NotificationContext notificationContext,
            ICardRepository cardRepository) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _userService = userService;
            _cardRepository = cardRepository;
        }

        public async Task SendCardDueDateChangedNotification(Guid cardId)
        {
            await RemovePendingCardJobs(cardId, NotificationType.CardDueDateUpdated);

            var card = await _cardRepository.Get(cardId);

            if (card == null || !card.DueDate.HasValue || !card.DueDateReminderType.HasValue
                || (card.DueDateReminderType.HasValue 
                && card.DueDateReminderType == DueDateReminderType.None))
            {
                return;
            }

            var delay = card.DueDate.Value - DateTime.Now;

            switch(card.DueDateReminderType)
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

            if(string.IsNullOrEmpty(jobId))
            {
                return;
            }

            _notificationContext.Notifications.Add(new Notification()
            {
                CardId = cardId,
                JobId = jobId,
                Type = NotificationType.CardDueDateUpdated,
                DateCreated = DateTime.Now,
            });

            await _notificationContext.SaveChangesAsync();
        }

        public async Task Notify(Guid cardId)
        {
            var filter = new CardNotificationSpecification(cardId);

            var card = await _cardRepository
                .Entities.Specify(filter)
                .FirstOrDefaultAsync();

            if (card == null || !card.DueDate.HasValue || card.BoardList.CardStatus == BoardListCardStatus.DONE)
            {
                return;
            }

            var subject = $"{card.Title} in {card.BoardList.Board.Title} is due {GetDueMessage(card.DueDateReminderType.Value)}";
            var cardMembers = (await _userService.GetAllAsync(card.Members.Select(m => m.UserId))).Data;

            foreach ( var member in cardMembers )
            {
                var content = $"Dear {member.FirstName} {member.LastName}, {card.Title} is due {GetDueMessage(card.DueDateReminderType.Value)}. " +
                    $"<br/> <strong>Card due date</strong>: {CardHelper.DisplayDates(card.StartDate, card.DueDate)}";
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
