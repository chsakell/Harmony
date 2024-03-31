using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Workspaces.Commands.Rename
{
    /// <summary>
    /// Command for renaming a workspace
    /// </summary>
    public class RenameWorkspaceStatusCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
