using System.Text.Json.Serialization;

namespace Harmony.Integrations.SourceControl.WebhookRequests.GitHub
{
    public class GitHubPullRequestDetails
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        [JsonPropertyName("diff_url")]
        public string DiffUrl { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("closed_at")]
        public DateTime? ClosedAt { get; set; }

        [JsonPropertyName("merged_at")]
        public DateTime? MergedAt { get; set; }

        [JsonPropertyName("merge_commit_sha")]
        public string MergeCommitSha { get; set; }

        [JsonPropertyName("assignee")]
        public GitHubUser Assignee { get; set; }

        [JsonPropertyName("assignees")]
        public List<GitHubUser> Assignees { get; set; }

        [JsonPropertyName("requested_reviewers")]
        public List<GitHubUser> RequestedReviewers { get; set; }

        [JsonPropertyName("head")]
        public GitHubPoint Head { get; set; }

        [JsonPropertyName("base")]
        public GitHubPoint Base { get; set; }
        public object merged_by { get; set; }
    }
}
