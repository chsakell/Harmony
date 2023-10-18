using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Client.Infrastructure.Models.Board
{
    public class EditableCheckListItemModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Description { get; set; }
        public Guid CheckListId { get; set; }
        public bool IsChecked { get; set; }
        public byte Position { get; set; }
        public DateTime? DueDate { get; set; }

        public MudDatePicker DatePicker;
    }
}
