
namespace Harmony.Domain.SourceControl
{
    public class PullRequest
    {
        public string Id { get; set; }
        public string HtmlUrl { get; set; }
        public string DiffUrl { get; set; }
        public string State { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public RepositoryUser Sender { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string ClosedAt { get; set; }
        public string MergedAt { get; set; }
        public string SourceBranch { get; set; }
        public string TargetBranch { get; set; }
        public List<RepositoryUser> Assignees { get; set; }
        public List<RepositoryUser> Reviewers { get; set; }
    }
}
