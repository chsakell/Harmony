using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Constants
{
    public class CacheKeys
    {
        public static string BoardInfo(Guid boardId) => $"board-info-{boardId}";
    }
}
