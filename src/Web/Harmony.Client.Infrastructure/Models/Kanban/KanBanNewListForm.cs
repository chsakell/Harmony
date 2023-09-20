using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Models.Kanban
{
	public class KanBanNewListForm
	{
		[Required]
		[StringLength(100, ErrorMessage = "Name length can't be more than 10.")]
		public string Name { get; set; }
	}
}
