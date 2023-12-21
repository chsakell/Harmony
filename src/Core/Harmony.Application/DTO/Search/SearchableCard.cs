using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO.Search
{
    public class SearchableCard
    {
        public string ObjectID { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Board { get; set; }
        public string List { get; set; }
        public string IssueType { get; set; }
    }
}
