using Harmony.Domain.Enums.Automations;
using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class AutomationEndpoints
    {
        public static string Index = $"{GatewayConstants.AutomationsApiPrefix}/automations";

        public static string Templates = $"{Index}/templates/";

        public static string Automation(string automationId)
            => $"{Index}/{automationId}/";

        public static string Automations(Guid boardId, AutomationType type) 
            => $"{Index}/{boardId}/types/{(int)type}/";

        public static string ToggleAutomation(string automationId)
            => $"{Index}/{automationId}/toggle/";
    }
}