using Hangfire;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Specifications.Cards;
using Harmony.Notifications.Contracts;
using Harmony.Notifications.Persistence;
using Harmony.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Harmony.Application.Contracts.Services.Identity;

namespace Harmony.Notifications.Services
{
    public class JobNotificationService : IJobNotificationService
    {
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly IUserService _userService;
        private readonly NotificationContext _notificationContext;
        private readonly ICardRepository _cardRepository;

        public JobNotificationService(IEmailNotificationService emailNotificationService,
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
            var card = await _cardRepository.Get(cardId);

            if (card == null)
            {
                return;
            }

            var delay = card.DueDate - DateTime.Now;

            var jobId = BackgroundJob.Schedule(() => Notify(cardId), TimeSpan.FromSeconds(30));

            _notificationContext.Notifications.Add(new Notification()
            {
                JobId = jobId,
                Type = Application.Enums.NotificationType.CardChangedDueDate,
                DateCreated = DateTime.Now,
            });
        }

        public async Task Notify(Guid cardId)
        {
            var filter = new CardNotificationSpecification(cardId);

            var card = await _cardRepository
                .Entities.Specify(filter)
                .FirstOrDefaultAsync();

            var subject = $"{card.Title} in {card.BoardList.Board.Title} is due in a day";

            var cardMembers = (await _userService.GetAllAsync(card.Members.Select(m => m.UserId))).Data;

            foreach ( var member in cardMembers )
            {
                await _emailNotificationService.SendEmailAsync(member.Email, subject, subject);
            }
            
        }
    }
}
