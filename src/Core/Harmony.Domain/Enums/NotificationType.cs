using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Enums
{
    public enum NotificationType
    {
        CardMoved,
        CardCreated,
        CardTitleChanged,
        CardDescriptionChanged,
        CardDatesChanged,
        CardItemAdded,
        CardIssueTypeChanged,
        BoardListTitleChanged,
        BoardListsPositionChanged,
        BoardListArchived,
        SubTaskStoryPoints,
        BoardListCreated,
        CardItemCheckedChanged
    }
}
