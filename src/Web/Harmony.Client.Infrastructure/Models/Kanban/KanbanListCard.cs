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
		public string Status { get; set; }

		public KanbanListCard(Guid id, string name, byte position, string status)
		{
			Id = id;
			Name = name;
			Status = status;
			Position = position;
		}
	}
}
