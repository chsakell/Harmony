using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.SourceControl.DTO
{
    public class BranchDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public RepositoryUserDto Creator { get; set; }
        public List<CommitDto> Commits { get; set; }
        public List<PullRequestDto> PullRequests { get; set; }

        // Extra properties
        public string RepositoryUrl { get; set; }
        public string RepositoryName { get; set; }
        public string BranchUrl => $"{RepositoryUrl}/tree/{Name}";
        public string CommitsUrl => $"{RepositoryUrl}/commits/{Name}";
        public SourceControlProvider Provider { get; set; }
        public List<string> Tags { get; set; }
    }
}
