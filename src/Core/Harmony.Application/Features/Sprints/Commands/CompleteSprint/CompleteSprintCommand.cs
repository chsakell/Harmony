using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Sprints.Commands.CompleteSprint
{
    /// <summary>
    /// Command to Complete a sprint
    /// </summary>
    public class CompleteSprintCommand : IRequest<Result<bool>>
    {
        [Required]
        public Guid BoardId { get; set; }
        public Guid SprintId { get; set; }
        public bool CreateNewSprint { get; set; }
        public bool MoveToBacklog { get; set; }
        public Guid? MoveToSprintId { get; set; }

        public CompleteSprintCommand(Guid boardId, Guid sprintId)
        {
            BoardId = boardId;
            SprintId = sprintId;
        }
    }
}
