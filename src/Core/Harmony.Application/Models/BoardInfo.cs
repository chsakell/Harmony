using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Models
{
    public class BoardInfo
    {
        public string Title { get; set; }
        public Dictionary<Guid, string> Lists { get; set; }
    }
}
