using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Models
{
    public abstract class BaseBoardCommand
    {
        public Guid BoardId { get; set; }
    }
}
