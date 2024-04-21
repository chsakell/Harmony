using System.Text.Json.Serialization;

namespace Harmony.Integrations.SourceControl.WebhookRequests.GitHub
{
    public class GitHubUser
    {
        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }
    }
}
