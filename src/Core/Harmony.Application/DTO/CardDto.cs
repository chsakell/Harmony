using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO
{
	public class CardDto
	{
		public string Name { get; set; }
		public string Description { get; set; }
		//public BoardList BoardList { get; set; }
		public Guid BoardListId { get; set; }
		public byte Position { get; set; } // position on the board list
		//public List<Comment> Comments { get; set; }
		//public List<CheckList> CheckLists { get; set; }
		//public List<UserCard> Members { get; set; }
		//public List<CardActivity> Activities { get; set; }
		//public bool IsArchived { get; set; }
		//public List<CardLabel> Labels { get; set; }
		//public DateTime? DueDate { get; set; }
		//public DateTime? ReminderDate { get; set; }
		//public List<Attachment> Attachments { get; set; }
	}
}
