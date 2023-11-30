using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Notifications.Contracts
{
    public interface IEmailNotificationService
    {
        Task SendEmailAsync(string recipientAddress, string subject, string htmlMessage);
    }
}
