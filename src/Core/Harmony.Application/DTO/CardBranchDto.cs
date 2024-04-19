using Harmony.Domain.Enums.SourceControl;

namespace Harmony.Application.DTO
{
    public class CardBranchDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public SourceControlProvider Provider { get; set; }
    }
}
