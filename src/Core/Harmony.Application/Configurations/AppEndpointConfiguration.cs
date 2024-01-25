using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Configurations
{
    /// <summary>
    /// Application endpoints
    /// </summary>
    public class AppEndpointConfiguration
    {
        public string AutomationEndpoint { get; set; }
        public string HarmonyApiEndpoint { get; set; }
        public string FrontendUrls { get; set; }

        public string[] GetFrontEndUrls()
        {
            return FrontendUrls.Split(",");
        }

        public string FrontendUrl => GetFrontEndUrls().FirstOrDefault().TrimEnd('/');
    }
}