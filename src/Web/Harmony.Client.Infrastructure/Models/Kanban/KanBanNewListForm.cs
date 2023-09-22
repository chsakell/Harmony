using System.ComponentModel.DataAnnotations;

namespace Harmony.Client.Infrastructure.Models.Kanban
{
    public class KanBanNewListForm
	{
		[Required]
		[StringLength(100, ErrorMessage = "Name length can't be more than 10.")]
		public string Name { get; set; }
	}
}
