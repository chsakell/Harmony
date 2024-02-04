using Harmony.Application.DTO;
using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Notifications
{
    public class CardCreatedMessage
    {
        public CardCreatedMessage(Guid boardId, CardDto card, string userId)
        {
            BoardId = boardId;
            Card = card;
            UserId = userId;
        }

        public Guid BoardId { get; set; }
        public CardDto Card { get; set; }
        public string UserId { get; set; }
    }
}
