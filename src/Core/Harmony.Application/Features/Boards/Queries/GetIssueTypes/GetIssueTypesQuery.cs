using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.GetIssueTypes
{
    public class GetIssueTypesQuery : IRequest<IResult<List<IssueTypeDto>>>
    {
        public Guid BoardId { get; set; }
        public bool NormalOnly { get; set; } = true;

        public GetIssueTypesQuery(Guid boardId)
        {
            BoardId = boardId;
        }
    }
}