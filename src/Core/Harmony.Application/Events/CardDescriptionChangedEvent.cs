using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Events
{
    public class CardDescriptionChangedEvent
    {
        public Guid CardId { get; set; }
        public string Description { get; set; }

        public CardDescriptionChangedEvent(Guid cardId, string description)
        {
            CardId = cardId;
            Description = description;
        }
    }
}
