using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.DTO
{
    public class CreateCardDto
    {
        [Required]
        public string Title { get; set; }
        public bool NewTaskOpen { get; set; }
        public bool Creating { get; set; }
    }
}
