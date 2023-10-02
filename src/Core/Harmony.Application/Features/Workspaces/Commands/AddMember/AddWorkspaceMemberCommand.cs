using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Workspaces.Commands.AddMember
{
    public class AddWorkspaceMemberCommand : IRequest<Result<bool>>
    {
        public string UserId { get; set; }
        public Guid WorkspaceId { get; set; }
    }
}
