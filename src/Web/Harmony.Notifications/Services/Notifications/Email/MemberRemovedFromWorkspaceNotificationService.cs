using Hangfire;
using Harmony.Application.Contracts.Repositories;
using Harmony.Notifications.Persistence;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Enums;
using Harmony.Notifications.Contracts.Notifications.Email;
using Harmony.Application.Notifications.Email;

namespace Harmony.Notifications.Services.Notifications.Email
{
    public class MemberRemovedFromWorkspaceNotificationService : BaseNotificationService, IMemberRemovedFromWorkspaceNotificationService
    {
        private readonly IEmailService _emailNotificationService;
        private readonly IUserService _userService;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IBoardService _boardService;
        private readonly IBoardRepository _boardRepository;

        public MemberRemovedFromWorkspaceNotificationService(
            IEmailService emailNotificationService,
            IUserService userService,
            IWorkspaceRepository workspaceRepository,
            IUserNotificationRepository userNotificationRepository,
            IBoardService boardService,
            NotificationContext notificationContext,
            IBoardRepository boardRepository) : base(notificationContext)
        {
            _emailNotificationService = emailNotificationService;
            _userService = userService;
            _workspaceRepository = workspaceRepository;
            _userNotificationRepository = userNotificationRepository;
            _boardService = boardService;
            _boardRepository = boardRepository;
        }

        public async Task Notify(MemberRemovedFromWorkspaceNotification notification)
        {
            await RemovePendingWorkspaceJobs(notification.WorkspaceId, notification.UserId, EmailNotificationType.MemberRemovedFromWorkspace);

            var workspace = await _workspaceRepository.GetAsync(notification.WorkspaceId);

            if (workspace == null)
            {
                return;
            }

            var jobId = BackgroundJob.Enqueue(() =>
                Notify(notification.WorkspaceId, notification.UserId, notification.WorkspaceUrl));

            if (string.IsNullOrEmpty(jobId))
            {
                return;
            }

            _notificationContext.Notifications.Add(new Notification()
            {
                WorkspaceId = notification.WorkspaceId,
                JobId = jobId,
                Type = EmailNotificationType.MemberRemovedFromWorkspace,
                DateCreated = DateTime.Now,
            });

            await _notificationContext.SaveChangesAsync();
        }


        public async Task Notify(Guid workspaceId, string userId, string workspaceUrl)
        {
            var workspace = await _workspaceRepository.GetAsync(workspaceId);

            if (workspace == null)
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
                .GetForUser(user.Id, EmailNotificationType.MemberRemovedFromWorkspace);

            if (notificationRegistration == null)
            {
                return;
            }

            var subject = $"Access to {workspace.Name} workspace revoked";

            var content = $"Dear {user.FirstName} {user.LastName},<br/><br/>" +
                $"Your access to <strong>{workspace.Name}</strong> workspace has been revoked.";

            await _emailNotificationService.SendEmailAsync(user.Email, subject, content);
        }
    }
}
