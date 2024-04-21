using Harmony.Domain.Enums.SourceControl;

namespace Harmony.Application.SourceControl.DTO
{
    public class PullRequestDto
    {
        public string Id { get; set; }
        public string HtmlUrl { get; set; }
        public string DiffUrl { get; set; }
        public PullRequestState State { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? MergedAt { get; set; }
        public string SourceBranch { get; set; }
        public string TargetBranch { get; set; }
        public string MergeCommitSha { get; set; }
        public List<RepositoryUserDto> Assignees { get; set; }
        public List<RepositoryUserDto> Reviewers { get; set; }
    }
}
