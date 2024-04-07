using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Extensions
{
    public static class LinkTypeExtensions
    {
        public static LinkType? GetCounterPart(this LinkType linkType)
        {
            return linkType switch {

                LinkType.IsBlockedBy => LinkType.Blocks,
                LinkType.Blocks => LinkType.IsBlockedBy,
                LinkType.IsClonedBy => LinkType.Clones,
                LinkType.Clones => LinkType.IsClonedBy,
                LinkType.IsDuplicatedBy => LinkType.Duplicates,
                LinkType.Duplicates => LinkType.IsDuplicatedBy,
                LinkType.RelatesTo => null,
                _ => null,
                
            };
        }

        public static string GetDescription(this LinkType linkType)
        {
            return linkType switch
            {

                LinkType.IsBlockedBy => "Is blocked by",
                LinkType.Blocks => "blocks",
                LinkType.IsClonedBy => "Is cloned by",
                LinkType.Clones => "Clones",
                LinkType.IsDuplicatedBy => "Is duplicated by",
                LinkType.Duplicates => "Duplicates",
                LinkType.RelatesTo => "Relates",
                _ => null,

            };
        }
    }
}
