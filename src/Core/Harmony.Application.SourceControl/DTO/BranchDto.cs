
namespace Harmony.Application.SourceControl.DTO
{
    public class BranchDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RepositoryId { get; set; }
        public string RepositoryUrl { get; set; }
        public string BranchUrl => $"{RepositoryUrl}/branches{Name}";
    }
}
