using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;


namespace Harmony.Application.Features.SourceControl.Commands.CreateBranch
{
    public class CreateBranchCommand : IRequest<Result<bool>>
    {
        public string SerialKey { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public RepositoryUser Creator { get; set; }
        public Repository Repository { get; set; }
        public bool SkipRepositoryCheck { get; set; }
        public string HeadCommitId { get; set; }
    }
}
