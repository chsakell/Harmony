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
        public string CardId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public bool IsComplete { get; set; }
        public string BoardTitle { get; set; }
        public Guid BoardId { get; set; }
        public string List { get; set; }
        public string IssueType { get; set; }
        public string SerialKey { get; set; }
    }
}
