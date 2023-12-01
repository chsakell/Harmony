using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Enums
{
    public enum DueDateReminderType
    {
        None,
        AtDueDate,
        FiveMinutesBefore,
        TenMinutesBefore,
        FifteenMinutesBefore,
        OneHourBefore,
        TwoHoursBefore,
        OneDayBefore,
        TwoDaysBefore
    }
}
