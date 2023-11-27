using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Sprints.Commands.StartSprint
{
    /// <summary>
    /// Command to start a sprint
    /// </summary>
    public class StartSprintCommand : IRequest<Result<bool>>
    {
        [Required]
        public Guid BoardId { get; set; }

        public Guid SprintId { get; set; }

        public StartSprintCommand(Guid boardId, Guid sprintId)
        {
            BoardId = boardId;
            SprintId = sprintId;
        }
    }
}
