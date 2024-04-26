using System.Text.Json.Serialization;

namespace Harmony.Integrations.SourceControl.WebhookRequests.GitHub
{

    public class GitHubPullRequest
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("pull_request")]
        public GitHubPullRequestDetails PullRequest { get; set; }

        [JsonPropertyName("repository")]
        public GitHubRepository Repository { get; set; }

        [JsonPropertyName("sender")]
        public GitHubUser Sender { get; set; }
    }
}
