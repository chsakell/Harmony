using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Boards.Commands.CreateSprint
{
    /// <summary>
    /// Command to create sprint
    /// </summary>
    public class CreateEditSprintCommand : IRequest<Result<SprintDto>>
    {
        [Required]
        public Guid BoardId { get; set; }

        public Guid? SprintId { get; set; }

        public CreateEditSprintCommand(Guid boardId)
        {
            BoardId = boardId;
        }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Goal { get; set; }

        [Required]
        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }
    }
}
