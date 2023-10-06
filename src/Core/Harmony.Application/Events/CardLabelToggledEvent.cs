using Harmony.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Events
{
    public class CardLabelToggledEvent
    {
        public Guid CardId { get; set; }
        public LabelDto Label { get; set; }
        public CardLabelToggledEvent(Guid cardId, LabelDto label)
        {
            CardId = cardId;
            Label = label;
        }
    }
}
