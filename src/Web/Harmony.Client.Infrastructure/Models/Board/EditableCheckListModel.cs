using MudBlazor;
using System.ComponentModel.DataAnnotations;

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

        public double TotalProgress
        {
            get
            {
                var totalItems = Items.Count;
                var totalItemsChecked = Items.Where(item => item.IsChecked).Count();

                var totalProgress = ((double)totalItemsChecked / (double)Items.Count) * 100;
                return totalProgress;
            }
        }

        public Color ProgressColor => TotalProgress >= 99 ? Color.Success : 
            TotalProgress == 0 ? Color.Warning : Color.Info;
    }
}
