using Hangfire;
using Harmony.Domain.Enums;
using Harmony.Persistence.DbContext;

namespace Harmony.Notifications.Services
{
    public abstract class BaseNotificationService
    {
        protected readonly NotificationContext _notificationContext;

        public BaseNotificationService(NotificationContext notificationContext)
        {
            _notificationContext = notificationContext;
        }

        protected async Task RemovePendingCardJobs(Guid cardId, EmailNotificationType type)
        {
            // check if there are already pending jobs for this card and type
            var jobs = _notificationContext.Tasks
                .Where(n => n.CardId == cardId && n.Type == (int)type);

            if (jobs.Any())
            {
                foreach (var job in jobs)
                {
                    BackgroundJob.Delete(job.JobId); // cancels the job
                    _notificationContext.Tasks.Remove(job);
                }

                await _notificationContext.SaveChangesAsync();
            }
        }

        protected async Task RemovePendingCardJobs(Guid cardId, string userId, EmailNotificationType type)
        {
            // check if there are already pending jobs for this card and type
            var jobs = _notificationContext.Tasks
                .Where(n => n.CardId == cardId && n.Type == (int)type && n.UserId == userId);

            if (jobs.Any())
            {
                foreach (var job in jobs)
                {
                    BackgroundJob.Delete(job.JobId); // cancels the job
                    _notificationContext.Tasks.Remove(job);
                }

                await _notificationContext.SaveChangesAsync();
            }
        }

        protected async Task RemovePendingBoardJobs(Guid boardId, string userId, EmailNotificationType type)
        {
            // check if there are already pending jobs for this board and type
            var jobs = _notificationContext.Tasks
                .Where(n => n.BoardId == boardId && n.Type == (int)type && n.UserId == userId);

            if (jobs.Any())
            {
                foreach (var job in jobs)
                {
                    BackgroundJob.Delete(job.JobId); // cancels the job
                    _notificationContext.Tasks.Remove(job);
                }

                await _notificationContext.SaveChangesAsync();
            }
        }

        protected async Task RemovePendingWorkspaceJobs(Guid workspaceId, string userId, EmailNotificationType type)
        {
            // check if there are already pending jobs for this board and type
            var jobs = _notificationContext.Tasks
                .Where(n => n.WorkspaceId == workspaceId && n.Type == (int)type && n.UserId == userId);

            if (jobs.Any())
            {
                foreach (var job in jobs)
                {
                    BackgroundJob.Delete(job.JobId); // cancels the job
                    _notificationContext.Tasks.Remove(job);
                }

                await _notificationContext.SaveChangesAsync();
            }
        }
    }
}
