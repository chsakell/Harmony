using Harmony.Domain.Enums;

namespace Harmony.Application.DTO
{
    public class BoardListDto
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
        public short Position { get; set; } // position on the board
		public List<CardDto> Cards { get; set; }

		// helpers for kanban

        public BoardListStatus Status { get; set; }

        // manual setup for pagination
        public int TotalCards{ get; set; }
        public int TotalPages { get; set; }
        public bool TitleEditing { get; set; }
        public BoardListCardStatus? CardStatus { get; set; }
    }
}
