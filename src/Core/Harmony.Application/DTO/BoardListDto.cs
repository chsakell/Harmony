using Harmony.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.DTO
{
    public class BoardListDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
        public byte Position { get; set; } // position on the board
		public List<CardDto> Cards { get; set; }

		// helpers for kanban

        public CreateCardDto CreateCard { get; set; } = new CreateCardDto();
        public BoardListStatus Status { get; set; }
    }
}
