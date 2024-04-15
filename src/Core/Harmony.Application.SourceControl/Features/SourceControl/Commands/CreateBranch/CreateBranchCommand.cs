using Harmony.Domain.Enums.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;


namespace Harmony.Application.Features.SourceControl.Commands.CreateBranch
{
    public class CreateBranchCommand : IRequest<Result<bool>>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RepositoryId { get; set; }
        public string RepositoryUrl { get; set; }
        public string RepositoryName { get; set; }
        public string RepositoryFullName { get; set; }
        public SourceControlProvider Provider { get; set; }
    }
}
