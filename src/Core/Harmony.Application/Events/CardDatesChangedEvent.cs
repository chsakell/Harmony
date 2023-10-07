using Harmony.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Events
{
    public class CardDatesChangedEvent
    {
        public CardDatesChangedEvent(Guid cardId, DateTime? startDate, DateTime? dueDate)
        {
            CardId = cardId;
            StartDate = startDate;
            DueDate = dueDate;
        }

        public Guid CardId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
