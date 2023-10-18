using System.ComponentModel.DataAnnotations;

namespace Harmony.Client.Infrastructure.Models.Labels
{
    public class CreateLabelModel
    {
        public string Title { get; set; }

        [Required]
        public string Color { get; set; }

        public bool IsChecked { get; set; }
    }
}
