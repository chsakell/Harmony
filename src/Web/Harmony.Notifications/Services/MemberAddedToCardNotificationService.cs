using Hangfire;
using Harmony.Application.Contracts.Repositories;
using Harmony.Notifications.Contracts;
using Harmony.Notifications.Persistence;
using Harmony.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Specifications.Boards;
using Harmony.Application.Notifications;
using Harmony.Domain.Enums;

namespace Harmony.Notifications.Services
{
    public class MemberAddedToCardNotificationService : BaseNotificationService, IMemberAddedToCardNotificationService
    {
        private readonly IEmailService _emailNotificationService;
        private readonly IUserService _userService;
        private readonly ICardRepository _cardRepository;
        private readonly IUserCardRepository _userCardRepository;
        private readonly IBoardRepository _boardRepository;

        public MemberAddedToCardNotificationService(
            IEmailService emailNotificationService,
            IUserService userService,
            ICardRepository cardRepository,
            IUserCardRepository userCardRepository,
            NotificationContext notificationContext,
            IBoardRepository boardRepository) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _userService = userService;
            _cardRepository = cardRepository;
            _userCardRepository = userCardRepository;
            _boardRepository = boardRepository;
        }

        public async Task Notify(MemberAddedToCardNotification notification)
        {
            await RemovePendingCardJobs(notification.CardId, notification.UserId, NotificationType.MemberAddedToCard);

            var userCard = await _userCardRepository
                .GetUserCard(notification.CardId, notification.UserId);

            if (userCard == null)
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
                Type = NotificationType.MemberAddedToCard,
                DateCreated = DateTime.Now,
            });

            await _notificationContext.SaveChangesAsync();
        }


        public async Task SendEmail(MemberAddedToCardNotification notification)
        {
            var filter = new BoardFilterSpecification(notification.BoardId, new BoardIncludes());

            var board = await _boardRepository
                .Entities.Specify(filter)
                .FirstOrDefaultAsync();

            if (board == null)
            {
                return;
            }

            var card = await _cardRepository.Get(notification.CardId);

            if (card == null)
            {
                return;
            }

            var userResult = await _userService.GetAsync(notification.UserId);

            if (!userResult.Succeeded || !userResult.Data.IsActive)
            {
                return;
            }

            var user = userResult.Data;

            var subject = $"Assigned to {card.Title} in {board.Title}";

            var content = $"Dear {user.FirstName} {user.LastName},<br/><br/>" +
                $"You have been assigned to <a href='{notification.CardUrl}' target='_blank'>{card.Title}</a> on {board.Title}.";

            await _emailNotificationService.SendEmailAsync(user.Email, subject, content);
        }
    }
}
