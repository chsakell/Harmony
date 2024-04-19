using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;


namespace Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePullRequest
{
    public class CreatePullRequestCommand : IRequest<Result<bool>>
    {
        public string PullRequestId { get; set; }
        public string HtmlUrl { get; set; }
        public string DiffUrl { get; set; }
        public string Action { get; set; }
        public string State { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public string SenderLogin { get; set; }
        public string SenderId { get; set; }
        public string SenderAvatarUrl { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string ClosedAt { get; set; }
        public string MergedAt { get; set; }
        public string SourceBranch { get; set; }
        public string TargetBranch { get; set; }
    }
}
