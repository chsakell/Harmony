using System.Text.Json.Serialization;

namespace Harmony.Integrations.SourceControl.WebhookRequests.GitHub
{
    public class GitHubBranchRequest
    {
        [JsonPropertyName("ref")]
        public string Ref { get; set; }

        [JsonPropertyName("master_branch")]
        public string SourceBranch { get; set; }

        [JsonPropertyName("ref_type")]
        public string Type { get; set; }

        [JsonPropertyName("repository")]
        public GitHubRepository Repository { get; set; }

        [JsonPropertyName("sender")]
        public GitHubUser Sender { get; set; }
    }
}
