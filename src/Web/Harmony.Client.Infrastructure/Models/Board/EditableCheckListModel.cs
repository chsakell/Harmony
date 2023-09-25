using Harmony.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Models.Board
{
    public class EditableCheckListModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid CardId { get; set; }
        public List<EditableCheckListItemModel> Items { get; set; }
        public byte Position { get; set; }

        // helper for add new items
        public EditableCheckListItemModel NewItem { get; set; }
    }
}
