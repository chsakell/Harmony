
using Harmony.Domain.Enums.SourceControl;

namespace Harmony.Domain.SourceControl
{
    public class Repository
    {
        public string Id { get; set; }
        public string RepositoryId { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public SourceControlProvider Provider { get; set; }
    }
}
