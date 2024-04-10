using Harmony.Application.DTO;
using Harmony.Application.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Extensions
{
    public static class LinkMessageExtensions
    {
        public static CardLinkCreatedMessage GetCreateMessageFromLink(LinkDto link)
        {
            return new CardLinkCreatedMessage(
                    link.Id,
                    sourceCardBoard : link.SourceCardBoard,
                    sourceCardId : link.SourceCardId,
                    sourceCardTitle : link.SourceCardTitle,
                    sourceCardSerialKey : link.SourceCardSerialKey,
                    targetCardBoard : link.TargetCardBoard,
                    targetCardId : link.TargetCardId,
                    targetCardTitle : link.TargetCardTitle,
                    targetCardSerialKey : link.TargetCardSerialKey,
                    userId : link.UserId,
                    type : link.Type
                    //dateCreated : link.DateCreated
                    );
        }

        public static LinkDto GetLinkFromCreateMessage(CardLinkCreatedMessage link)
        {
            return new LinkDto()
            {
                Id = link.Id,
                SourceCardBoard = link.SourceCardBoard,
                SourceCardId = link.SourceCardId,
                SourceCardTitle = link.SourceCardTitle,
                SourceCardSerialKey = link.SourceCardSerialKey,
                TargetCardBoard = link.TargetCardBoard,
                TargetCardId = link.TargetCardId,
                TargetCardTitle = link.TargetCardTitle,
                TargetCardSerialKey = link.TargetCardSerialKey,
                UserId = link.UserId,
                Type = link.Type,
                //DateCreated = link.DateCreated
            };
        }
    }
}
