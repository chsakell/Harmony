using Harmony.Application.Specifications.Base;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Specifications.Sprints
{
    public class LinkFilterSpecification : HarmonySpecification<Link>
    {
        public Guid? LinkId { get; set; }
        public Guid? SourceCardId { get; set; }
        public Guid? TargetCardId { get; set; }
        public LinkType? Type { get; set; }

        public void Build()
        {

            if (LinkId.HasValue)
            {
                Criteria = And(link => link.Id == LinkId);
            }

            if (SourceCardId.HasValue)
            {
                Criteria = And(link => link.SourceCardId == SourceCardId);
            }

            if (TargetCardId.HasValue)
            {
                Criteria = And(link => link.TargetCardId == TargetCardId);
            }

            if (Type.HasValue)
            {
                Criteria = And(link => link.Type == Type);
            }
        }
    }
}
