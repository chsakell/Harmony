using MudBlazor;
using MudBlazor.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
