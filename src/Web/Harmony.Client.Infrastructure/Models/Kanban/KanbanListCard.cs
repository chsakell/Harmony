using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Models.Kanban
{
	public class KanbanListCard
	{
		public string Name { get; init; }
		public string Status { get; set; }

		public KanbanListCard(string name, string status)
		{
			Name = name;
			Status = status;
		}
	}
}
