using Harmony.Application.SourceControl.DTO;
using Harmony.Domain.SourceControl;

namespace Harmony.Application.SourceControl.Messages
{
    public class BranchCommitsPushedMessage
    {
        public string SerialKey { get; set; }
        public string Branch { get; set; }
        public List<CommitDto> Commits { get; set; }
        public RepositoryUserDto Sender { get; set; }
    }
}
