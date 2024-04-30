using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;


namespace Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePush
{
    public class CreatePushCommand : IRequest<Result<bool>>
    {
        public string SerialKey { get; set; }
        public string Ref { get; set; }
        public Repository Repository { get; set; }
        public List<Commit> Commits { get; set; }
        public RepositoryUser Sender { get; set; }
        public Commit HeadCommit { get; set; }
        public bool TagPushed { get; set; }
        public bool BranchPushed { get; set; }
    }
}
