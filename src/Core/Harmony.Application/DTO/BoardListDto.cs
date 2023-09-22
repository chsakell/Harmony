using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO
{
	public class BoardListDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
        public byte Position { get; set; } // position on the board
		public List<CardDto> Cards { get; set; }

		// helpers for kanban
        public string NewCardName { get; set; }
        public bool NewTaskOpen { get; set; }
		public BoardListStatus Status { get; set; }
    }
}
