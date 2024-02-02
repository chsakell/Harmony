using Harmony.Domain.Enums;
using Harmony.Domain.Enums.Automations;

namespace Harmony.Domain.Automation
{
    public class AutomationTemplate
    {
        public string Id { get; set; }
        public string Summary { get; set; }
        public string Name { get; set; }
        public AutomationType Type { get; set; }
        public bool Enabled { get; set; }
        public List<NotificationType> NotificationTypes { get; set; }
    }
}
