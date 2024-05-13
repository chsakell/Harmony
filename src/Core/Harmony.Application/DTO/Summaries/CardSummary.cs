using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO.Summaries
{
    public class CardSummary
    {
        public Guid CardId { get; set; }
        public List<Guid> Labels { get; set; } = new List<Guid>();
        public List<CheckListSummary> CheckLists { get; set; } = new List<CheckListSummary>();
        public int TotalChildren { get; set; }
        public List<string> Members { get; set; } = new List<string>();
        public int TotalLinks { get; set; }
        public int TotalComments { get; set; }
        public int TotalAttachments { get; set; }
    }
}
