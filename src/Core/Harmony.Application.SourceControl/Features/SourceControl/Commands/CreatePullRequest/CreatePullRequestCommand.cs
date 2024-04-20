using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;


namespace Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePullRequest
{
    public class CreatePullRequestCommand : IRequest<Result<bool>>
    {
        public string Id { get; set; }
        public string Action { get; set; }
        public string HtmlUrl { get; set; }
        public string DiffUrl { get; set; }
        public PullRequestState State { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public Repository Repository { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? MergedAt { get; set; }
        public string SourceBranch { get; set; }
        public string TargetBranch { get; set; }
        public string MergeCommitSha { get; set; }
        public List<RepositoryUser> Assignees { get; set; }
        public List<RepositoryUser> Reviewers { get; set; }
    }
}
