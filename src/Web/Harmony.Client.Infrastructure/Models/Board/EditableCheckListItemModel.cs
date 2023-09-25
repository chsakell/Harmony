using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Models.Board
{
    public class EditableCheckListItemModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public Guid CheckListId { get; set; }
        public bool IsChecked { get; set; }
        public byte Position { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
