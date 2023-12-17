using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO
{
    public class IssuesOverviewReportDto
    {
        public List<double> TotalIssuesPerType { get; set; } = new List<double>();
        public List<string> IssueTypes { get; set; } = new List<string>();
        public int TotalIssues { get; set; }
    }
}
