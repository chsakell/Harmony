using Harmony.Application.DTO;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.MoveToSprint
{
    /// <summary>
    /// Command to move items to a sprint
    /// </summary>
    public class MoveToSprintCommand : IRequest<Result<SprintDto>>
    {
        [Required]
        public Guid BoardId { get; set; }

        public MoveToSprintCommand(Guid boardId)
        {
            BoardId = boardId;
        }
    }
}
