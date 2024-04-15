
using Harmony.Domain.Enums.SourceControl;

namespace Harmony.Domain.SourceControl
{
    public class Branch
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RepositoryId { get; set; }
        //public string BranchUrl => $"{RepositoryUrl}/branches{Name}";
    }
}
