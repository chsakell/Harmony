using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Events
{
    public class CardItemAddedEvent
    {
        public Guid CardId { get; set; }

        public CardItemAddedEvent(Guid cardId)
        {
            CardId = cardId;
        }
    }
}
