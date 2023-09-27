using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO
{
    public class CreateCardDto
    {
        [Required]
        public string Name { get; set; }
        public bool NewTaskOpen { get; set; }
    }
}
