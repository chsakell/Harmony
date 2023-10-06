using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Events
{
    public class CardTitleChangedEvent
    {
        public Guid CardId { get; set; }
        public string Title { get; set; }

        public CardTitleChangedEvent(Guid cardId, string title)
        {
            CardId = cardId;
            Title = title;
        }
    }
}
