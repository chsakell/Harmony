using System.Text.Json.Serialization;

namespace Harmony.Integrations.SourceControl.WebhookRequests.GitHub
{
    public class GitHubPoint
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }


        [JsonPropertyName("ref")]
        public string Ref { get; set; }

        [JsonPropertyName("sha")]
        public string Sha { get; set; }
    }
}
