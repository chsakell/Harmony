using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Enums
{
    public enum CardActivityType
    {
        CardAdded,
        CardArchived,
        CardTitleUpdated,
        CardDescriptionUpdated,
        AttachmentAdded,
        AttachmentRemoved,
        CheckListAdded,
        CheckListItemAdded,
        CheckListItemCompleted,
        CheckListItemPending,
        CommentAdded,
        CardDatesUpdated
    }
}
