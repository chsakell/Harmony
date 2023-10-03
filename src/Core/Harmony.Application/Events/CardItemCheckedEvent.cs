using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Events
{
    public class CardItemCheckedEvent
    {
        public Guid CardId { get; set; }
        public Guid CheckListItemId { get; set; }
        public bool IsChecked { get; set; }

        public CardItemCheckedEvent(Guid cardId, Guid checkListItemId, bool isChecked)
        {
            CardId = cardId;
            CheckListItemId = checkListItemId;
            IsChecked = isChecked;
        }
    }
}
