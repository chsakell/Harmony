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
using Harmony.Infrastructure.Repositories;

namespace Harmony.Notifications.Services
{
    public class MemberAddedToBoardNotificationService : BaseNotificationService, IMemberAddedToBoardNotificationService
    {
        private readonly IEmailService _emailNotificationService;
        private readonly IUserService _userService;
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IBoardService _boardService;
        private readonly IBoardRepository _boardRepository;

        public MemberAddedToBoardNotificationService(
            IEmailService emailNotificationService,
            IUserService userService,
            IUserNotificationRepository userNotificationRepository,
            IBoardService boardService,
            NotificationContext notificationContext,
            IBoardRepository boardRepository) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _userService = userService;
            _userNotificationRepository = userNotificationRepository;
            _boardService = boardService;
            _boardRepository = boardRepository;
        }

        public async Task Notify(MemberAddedToBoardNotification notification)
        {
            await RemovePendingCardJobs(notification.BoardId, NotificationType.MemberAddedToBoard);

            var board = await _boardRepository.GetAsync(notification.BoardId);

            if (board == null)
            {
                return;
            }

            var jobId = BackgroundJob.Enqueue(() => Notify(notification.BoardId, notification.UserId, notification.BoardUrl));

            if (string.IsNullOrEmpty(jobId))
            {
                return;
            }

            _notificationContext.Notifications.Add(new Notification()
            {
                BoardId = notification.BoardId,
                JobId = jobId,
                Type = NotificationType.MemberAddedToBoard,
                DateCreated = DateTime.Now,
            });

            await _notificationContext.SaveChangesAsync();
        }


        public async Task Notify(Guid boardId, string userId, string boardUrl)
        {
            var filter = new BoardFilterSpecification(boardId, new BoardIncludes()
            {
                Workspace = true
            });

            var board = await _boardRepository
                .Entities.Specify(filter)
                .FirstOrDefaultAsync();

            if (board == null)
            {
                return;
            }

            var userResult = await _userService.GetAsync(userId);

            if (!userResult.Succeeded || !userResult.Data.IsActive)
            {
                return;
            }
            var user = userResult.Data;

            var notificationRegistration = await _userNotificationRepository
                .GetForUser(user.Id, NotificationType.MemberAddedToBoard);

            if (notificationRegistration == null)
            {
                return;
            }

            var userHasAccessToBoard = await _boardService.HasUserAccessToBoard(userId, boardId);

            if (!userHasAccessToBoard)
            {
                return;
            }

            var subject = $"Access {board.Title} in {board.Workspace.Name}";

            var content = $"Dear {user.FirstName} {user.LastName},<br/><br/>" +
                $"You can now access <a href='{boardUrl}' target='_blank'>{board.Title}</a> on {board.Workspace.Name} workspace.";

            await _emailNotificationService.SendEmailAsync(user.Email, subject, content);
        }
    }
}
