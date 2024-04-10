using Harmony.Application.DTO;
using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications
{
    public class CardLinkCreatedMessage
    {
        public CardLinkCreatedMessage(Guid id, 
            BoardDto sourceCardBoard, 
            Guid sourceCardId, 
            string sourceCardTitle, 
            string sourceCardSerialKey, 
            //BoardDto targetCardBoard, 
            Guid targetCardId, 
            string targetCardTitle, 
            string targetCardSerialKey, 
            string userId, 
            LinkType type)
        {
            Id = id;
            SourceCardBoard = sourceCardBoard;
            SourceCardId = sourceCardId;
            SourceCardTitle = sourceCardTitle;
            SourceCardSerialKey = sourceCardSerialKey;
            //TargetCardBoard = targetCardBoard;
            TargetCardId = targetCardId;
            TargetCardTitle = targetCardTitle;
            TargetCardSerialKey = targetCardSerialKey;
            UserId = userId;
            Type = type;
        }

        public Guid Id { get; set; }
        public BoardDto SourceCardBoard { get; set; }
        public Guid SourceCardId { get; set; }
        public string SourceCardTitle { get; set; }
        public string SourceCardSerialKey { get; set; }
        //public BoardDto TargetCardBoard { get; set; }
        public Guid TargetCardId { get; set; }
        public string TargetCardTitle { get; set; }
        public string TargetCardSerialKey { get; set; }
        public string UserId { get; set; }
        public LinkType Type { get; set; }
    }
}
