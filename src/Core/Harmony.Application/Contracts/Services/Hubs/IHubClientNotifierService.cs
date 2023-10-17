using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Services.Hubs
{
    public interface IHubClientNotifierService
    {
        Task UpdateCardTitle(Guid boardId, Guid cardId, string title);
    }
}
