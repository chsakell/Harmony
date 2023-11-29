using Harmony.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Notifications
{
    public abstract class BaseNotification
    {
        public abstract NotificationType Type { get; }
    }
}
