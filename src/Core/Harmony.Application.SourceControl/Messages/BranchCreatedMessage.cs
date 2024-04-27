using Harmony.Application.SourceControl.DTO;
using Harmony.Domain.Enums.SourceControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.SourceControl.Messages
{
    public class BranchCreatedMessage
    {
        public string SerialKey { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public RepositoryUserDto Creator { get; set; }

        // Extra properties
        public string RepositoryUrl { get; set; }
        public string RepositoryName { get; set; }
        public string BranchUrl => $"{RepositoryUrl}/tree/{Name}";
        public string CommitsUrl => $"{RepositoryUrl}/commits/{Name}";
        public SourceControlProvider Provider { get; set; }
    }
}
