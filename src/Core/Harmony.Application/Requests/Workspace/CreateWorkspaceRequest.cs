using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Requests.Workspace
{
    public class CreateWorkspaceRequest
    {
        [Required]
        [MaxLength(300)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}
