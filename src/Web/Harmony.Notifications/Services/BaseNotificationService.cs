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

        protected async Task RemovePendingJobs(Guid cardId, NotificationType type)
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
    }
}
