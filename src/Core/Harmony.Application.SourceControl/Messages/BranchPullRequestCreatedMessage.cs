using Harmony.Application.SourceControl.DTO;
using Harmony.Domain.SourceControl;

namespace Harmony.Application.SourceControl.Messages
{
    public class BranchPullRequestCreatedMessage
    {
        public string SerialKey { get; set; }
        public PullRequestDto PullRequest { get; set; }
        public RepositoryUserDto Sender { get; set; }
    }
}
