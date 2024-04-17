using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.SourceControl.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
