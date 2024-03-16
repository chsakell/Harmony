using Harmony.Application.DTO;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Workspaces.Commands.UpdateStatus
{
    /// <summary>
    /// Command for updating workspace status
    /// </summary>
    public class UpdateWorkspaceStatusCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }

        public WorkspaceStatus Status { get; set; }
    }
}
