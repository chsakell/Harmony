using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Lists.Commands.CreateList
{
    public class CreateListCommand : IRequest<Result<BoardListDto>>
    {
        [Required]
        public string Name { get; set; }

        public Guid BoardId { get; set; }

        public CreateListCommand(string name, Guid boardId)
        {
            Name = name;
            BoardId = boardId;
        }
    }
}
