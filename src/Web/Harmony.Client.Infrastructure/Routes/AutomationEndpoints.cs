using Harmony.Domain.Enums;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class AutomationEndpoints
    {
        public static string Index = "api/automations";

        public static string Templates = $"{Index}/templates/";
    }
}