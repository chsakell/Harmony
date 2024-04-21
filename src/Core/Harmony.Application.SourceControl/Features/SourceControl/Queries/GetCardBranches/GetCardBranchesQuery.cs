using Harmony.Application.SourceControl.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.SourceControl.Features.SourceControl.Queries.GetCardBranches
{
    public class GetCardBranchesQuery : IRequest<IResult<List<BranchDto>>>
    {
        public GetCardBranchesQuery(string serialKey)
        {
            SerialKey = serialKey;
        }

        public string SerialKey { get; set; }
    }
}
