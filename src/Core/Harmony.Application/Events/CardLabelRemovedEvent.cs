using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Events
{
    public class CardLabelRemovedEvent
    {
        public Guid CardLabelId { get; set; }

        public CardLabelRemovedEvent(Guid cardLabelId)
        {
            CardLabelId = cardLabelId;
        }
    }
}
