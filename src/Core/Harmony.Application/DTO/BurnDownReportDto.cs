using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO
{
    public class BurnDownReportDto
    {
        public string Name { get; set; }
        public List<string> Dates { get; set; }
        public List<double> GuideLineStoryPoints { get; set; }
        public List<double> RemainingStoryPoints { get; set; }
    }
}
