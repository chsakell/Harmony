using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO.Summaries
{
    public class CheckListSummary
    {
        public Guid CheckListId { get; set; }
        public int TotalItems { get; set; }
        public int TotalItemsChecked { get; set; }
    }
}
