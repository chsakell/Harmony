using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Workspaces.Commands.Create
{
    /// <summary>
    /// Command for creating workspace
    /// </summary>
    public class CreateWorkspaceCommand : IRequest<Result<WorkspaceDto>>
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsPublic { get; set; }
    }
}
