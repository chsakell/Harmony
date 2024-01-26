using Harmony.Application.DTO;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Boards.Commands.Create
{
    /// <summary>
    /// Command to create board
    /// </summary>
    public class CreateBoardCommand : IRequest<Result<BoardDto>>
    {
        [Required]
        [MaxLength(300)]
        [MinLength(5)]
        public string Title { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; }

        [Required]
        public string WorkspaceId { get; set; }
        public BoardVisibility Visibility { get; set; }

        [Required]
        public BoardType BoardType { get; set; }

        [Required]
        [MaxLength(5)]
        public string Key { get; set; }
    }
}
