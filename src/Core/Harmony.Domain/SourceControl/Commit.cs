using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.SourceControl
{
    public class Commit
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }
        public string Url { get; set; }
        public string CommiterName { get; set; }
        public string CommiterEmail { get; set; }
        public string CommiterUsername { get; set; }
        public List<string> Added {  get; set; }
        public List<string> Removed { get; set; }
        public List<string> Modified { get; set; }
    }
}
