using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Notifications.Models
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public Message(IEnumerable<MailboxAddress> to, string subject, string content)
        {
            To = to.ToList();
            Subject = subject;
            Content = content;
        }

        public Message(MailboxAddress to, string subject, string content)
        {
            To = new List<MailboxAddress>
            {
                to
            };
            Subject = subject;
            Content = content;
        }
    }
}
