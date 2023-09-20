using Harmony.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Harmony.Client.Infrastructure.Models.Kanban
{
	public class KanBanList : BoardListDto
	{
		public bool NewTaskOpen { get; set; }

		public KanBanList(Guid id, string name, byte position)
		{
			Name = name;
			Id = id;
			Position = position;
		}
	}
}
