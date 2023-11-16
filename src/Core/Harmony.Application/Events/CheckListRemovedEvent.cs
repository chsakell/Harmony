using Harmony.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Events
{
    public class CheckListRemovedEvent
    {
        public Guid CheckListId { get; set; }
        public int CardId { get; set; }
        public int TotalItems { get; set; }
        public int TotalItemsCompleted { get; set; }
        public CheckListRemovedEvent(Guid checkListId, int cardId, int totalItems, int totalItemsCompleted)
        {
            CheckListId = checkListId;
            CardId = cardId;
            TotalItems = totalItems;
            TotalItemsCompleted = totalItemsCompleted;
        }
    }
}
