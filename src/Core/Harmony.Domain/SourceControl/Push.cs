using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.SourceControl
{
    public class Push
    {
        public string Id { get; set; }
        public string Ref { get; set; }
        public string RepositoryId { get; set; }
        public string PusherName { get; set; }
        public string PusherEmail { get; set; }
        public string SenderLogin { get; set; }
        public string SenderId { get; set; }
        public string SenderAvatarUrl { get; set; }
        public string CompareUrl { get; set; }
        public List<Commit> Commits { get; set; }
    }
}
