using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Commands.AddMember
{
    /// <summary>
    /// Command for adding workspace member
    /// </summary>
    public class AddWorkspaceMemberCommand : IRequest<Result<bool>>
    {
        public AddWorkspaceMemberCommand(string userId, Guid workspaceId)
        {
            UserId = userId;
            WorkspaceId = workspaceId;
        }

        public string UserId { get; set; }
        public Guid WorkspaceId { get; set; }
    }
}
