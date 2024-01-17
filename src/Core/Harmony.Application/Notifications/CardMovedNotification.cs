using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Notifications
{
    public class CardMovedNotification
    {
        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public Guid? ParentCardId { get; set; }
        public Guid MovedFromListId { get; set; }
        public Guid MovedToListId { get; set; }
        public bool IsCompleted { get; set; }
    }
}
