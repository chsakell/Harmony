using Harmony.Domain.Enums.SourceControl;

namespace Harmony.Application.DTO.SourceControl
{
    public class BranchDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RepositoryId { get; set; }
        public string RepositoryUrl { get; set; }
        public string BranchUrl => $"{RepositoryUrl}/tree/{Name}";
        public string CommitsUrl => $"{RepositoryUrl}/commits/{Name}";
        public SourceControlProvider Provider { get; set; }
    }
}
