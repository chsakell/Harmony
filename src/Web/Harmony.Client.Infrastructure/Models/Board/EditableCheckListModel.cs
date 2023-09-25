using Harmony.Application.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Models.Board
{
    public class EditableCheckListModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }
        public Guid CardId { get; set; }
        public List<EditableCheckListItemModel> Items { get; set; }
        public byte Position { get; set; }

        // helper for add new items
        public EditableCheckListItemModel NewItem { get; set; }
    }
}
