using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.MoveToBacklog
{
    /// <summary>
    /// Command to move items to backlog
    /// </summary>
    public class MoveToBacklogCommand : IRequest<IResult<List<CardDto>>>
    {
        public Guid BoardId { get; set; }
        public List<Guid> Cards { get; set; }

        public MoveToBacklogCommand(Guid boardId, List<Guid> cards)
        {
            BoardId = boardId;
            Cards = cards;
        }
    }
}
