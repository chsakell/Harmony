using Harmony.Application.Responses;

namespace Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers
{
    public class UserWorkspaceResponse : UserResponse
    {
        public bool IsMember { get; set; }
    }
}
