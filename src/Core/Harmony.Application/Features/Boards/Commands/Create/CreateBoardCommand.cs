using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Boards.Commands.Create
{
    public class CreateBoardCommand : IRequest<Result<Guid>>
    {
        [Required]
        [MaxLength(300)]
        public string Title { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; }

        [Required]
        public string WorkspaceId { get; set; }
        public BoardVisibility Visibility { get; set; }
    }
}
