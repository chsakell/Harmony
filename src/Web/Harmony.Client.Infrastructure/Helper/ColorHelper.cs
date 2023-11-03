using Harmony.Domain.Enums;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Helper
{
    public class ColorHelper
    {
        public static Color GetVisibilityColor(BoardVisibility visibility)
        {
            return visibility switch
            {
                BoardVisibility.Private => Color.Error,
                BoardVisibility.Public => Color.Success,
                BoardVisibility.Workspace => Color.Info,
                _ => Color.Info,
            };
        }
    }
}
