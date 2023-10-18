using Harmony.Application.Responses;

namespace Harmony.Application.Features.Workspaces.Queries.SearchWorkspaceUsers
{
    public class SearchWorkspaceUserResponse : UserResponse
    {
        public bool IsMember { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
