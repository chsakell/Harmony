using Hangfire;
using Harmony.Application.Enums;
using Harmony.Notifications.Persistence;

namespace Harmony.Notifications.Services
{
    public abstract class BaseNotificationService
    {
        protected readonly NotificationContext _notificationContext;

        public BaseNotificationService(NotificationContext notificationContext)
        {
            _notificationContext = notificationContext;
        }

        protected async Task RemovePendingCardJobs(Guid cardId, NotificationType type)
        {
            // check if there are already pending jobs for this card and type
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

        protected async Task RemovePendingCardJobs(Guid cardId, string userId, NotificationType type)
        {
            // check if there are already pending jobs for this card and type
            var jobs = _notificationContext.Notifications
                .Where(n => n.CardId == cardId && n.Type == type && n.UserId == userId);

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

        protected async Task RemovePendingBoardJobs(Guid boardId, NotificationType type)
        {
            // check if there are already pending jobs for this board and type
            var jobs = _notificationContext.Notifications
                .Where(n => n.BoardId == boardId && n.Type == type);

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
    }
}
