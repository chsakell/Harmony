using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO.Search
{
    public class IndexedCard
    {
        public string ObjectID { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string BoardId { get; set; }
        public Guid ListId { get; set; }
        public string IssueType { get; set; }
        public string SerialKey { get; set; }
    }
}
