namespace Harmony.Application.Configurations
{
    public class AlgoliaConfiguration
    {
        public string ApplicationId { get; set; }
        public string ApiKey { get; set; }
        public bool Enabled => !string.IsNullOrEmpty(ApplicationId) && !string.IsNullOrEmpty(ApiKey);
    }
}
