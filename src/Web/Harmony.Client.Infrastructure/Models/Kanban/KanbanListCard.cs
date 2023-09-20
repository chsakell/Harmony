using Harmony.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Models.Kanban
{
	public class KanbanListCard : CardDto
	{
		public KanbanListCard(Guid id, Guid boardListId, string name, byte position)
		{
			Id = id;
			BoardListId = boardListId;
			Name = name;
			Position = position;
		}
	}
}
