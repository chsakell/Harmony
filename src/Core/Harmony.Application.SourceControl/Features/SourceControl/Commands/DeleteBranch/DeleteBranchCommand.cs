using Harmony.Domain.Enums.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;


namespace Harmony.Application.Features.SourceControl.Commands.DeleteBranch
{
    public class DeleteBranchCommand : IRequest<Result<bool>>
    {
        public string Name { get; set; }
        public string RepositoryId { get; set; }
    }
}
