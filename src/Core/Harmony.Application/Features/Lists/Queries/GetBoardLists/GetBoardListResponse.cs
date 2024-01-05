using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Lists.Queries.GetBoardLists
{
    public class GetBoardListResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public short Position { get; set; } // position on the board
        public BoardListCardStatus? CardStatus { get; set; }
    }
}
