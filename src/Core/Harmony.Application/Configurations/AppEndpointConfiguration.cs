namespace Harmony.Application.Configurations
{
    /// <summary>
    /// Application endpoints
    /// </summary>
    public class AppEndpointConfiguration
    {
        public string AutomationEndpoint { get; set; }
        public string HarmonyApiEndpoint { get; set; }

        private string _frontendUrl;
        public string FrontendUrl
        {
            get { return _frontendUrl; }
            set { _frontendUrl = value.TrimEnd('/'); }
        }
    }
}