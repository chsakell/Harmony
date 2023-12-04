using Harmony.Application.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Services.UserNotifications
{
    public interface IUserNotificationService
    {
        Task HandleNotification<T>(string userId, T notification) where T : BaseNotification;
    }
}
