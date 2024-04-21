using System.Text.Json.Serialization;

namespace Harmony.Integrations.SourceControl.WebhookRequests.GitHub
{
    public class GitHubCommit
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("author")]
        public GitHubAuthor Author { get; set; }

        [JsonPropertyName("added")]
        public List<string> Added { get; set; }

        [JsonPropertyName("removed")]
        public List<string> Removed { get; set; }

        [JsonPropertyName("modified")]
        public List<string> Modified { get; set; }
    }
}
