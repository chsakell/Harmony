using Harmony.Domain.Enums;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class AutomationEndpoints
    {
        public static string Index = "api/automations";

        public static string Templates = $"{Index}/templates/";

        public static string Automation(string automationId)
            => $"{Index}/{automationId}/";

        public static string Automations(Guid boardId, AutomationType type) 
            => $"{Index}/{boardId}/types/{(int)type}/";

        public static string ToggleAutomation(string automationId)
            => $"{Index}/{automationId}/toggle/";
    }
}