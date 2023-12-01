using Hangfire;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Specifications.Cards;
using Harmony.Notifications.Contracts;
using Harmony.Notifications.Persistence;
using Harmony.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Domain.Entities;
using Harmony.Application.Enums;
using Harmony.Application.Helpers;

namespace Harmony.Notifications.Services
{
    public class CardDueDateNotificationService : ICardDueDateNotificationService
    {
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly IUserService _userService;
        private readonly NotificationContext _notificationContext;
        private readonly ICardRepository _cardRepository;

        public CardDueDateNotificationService(IEmailNotificationService emailNotificationService,
            IUserService userService,
            NotificationContext notificationContext,
            ICardRepository cardRepository)
        {
            _emailNotificationService = emailNotificationService;
            _userService = userService;
            _notificationContext = notificationContext;
            _cardRepository = cardRepository;
        }

        public async Task SendCardDueDateChangedNotification(Guid cardId)
        {
            await RemovePendingJobs(cardId, NotificationType.CardChangedDueDate);

            var card = await _cardRepository.Get(cardId);

            if (card == null || !card.DueDate.HasValue)
            {
                return;
            }

            var delay = card.DueDate - DateTime.Now.AddDays(-1);

            if(!delay.HasValue)
            {
                return;
            }

            var jobId = BackgroundJob.Schedule(() => Notify(cardId), delay.Value);

            if(string.IsNullOrEmpty(jobId))
            {
                return;
            }

            _notificationContext.Notifications.Add(new Notification()
            {
                CardId = cardId,
                JobId = jobId,
                Type = NotificationType.CardChangedDueDate,
                DateCreated = DateTime.Now,
            });

            await _notificationContext.SaveChangesAsync();
        }

        private async Task RemovePendingJobs(Guid cardId, NotificationType type)
        {
            // check if there are already pending jobs for this
            var jobs = _notificationContext.Notifications
                .Where(n => n.CardId == cardId && n.Type == type);

            if (jobs.Any())
            {
                foreach (var job in jobs)
                {
                    BackgroundJob.Delete(job.JobId); // cancels the job
                    _notificationContext.Notifications.Remove(job);
                }

                await _notificationContext.SaveChangesAsync();
            }
        }

        public async Task Notify(Guid cardId)
        {
            var filter = new CardNotificationSpecification(cardId);

            var card = await _cardRepository
                .Entities.Specify(filter)
                .FirstOrDefaultAsync();

            if (card == null || !card.DueDate.HasValue)
            {
                return;
            }

            var subject = $"{card.Title} in {card.BoardList.Board.Title} is due in a day";
            var cardMembers = (await _userService.GetAllAsync(card.Members.Select(m => m.UserId))).Data;

            foreach ( var member in cardMembers )
            {
                var content = $"Dear {member.FirstName} {member.LastName}, {card.Title} is due in a day. " +
                    $"<br/> <strong>Card due date</strong>: {CardHelper.DisplayDates(card.StartDate, card.DueDate)}";
                await _emailNotificationService.SendEmailAsync(member.Email, subject, content);
            }
            
        }
    }
}
