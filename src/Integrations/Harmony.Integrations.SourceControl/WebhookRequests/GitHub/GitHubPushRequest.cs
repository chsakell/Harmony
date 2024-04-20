using System.Text.Json.Serialization;

namespace Harmony.Integrations.SourceControl.WebhookRequests.GitHub
{

    public class GitHubPushRequest
    {
        [JsonPropertyName("ref")]
        public string Ref { get; set; }

        [JsonPropertyName("created")]
        public bool Created { get; set; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }

        [JsonPropertyName("forced")]
        public bool Forced { get; set; }

        [JsonPropertyName("repository")]
        public GitHubRepository Repository { get; set; }

        [JsonPropertyName("commits")]
        public GitHubCommit[] Commits { get; set; }
    }
}
