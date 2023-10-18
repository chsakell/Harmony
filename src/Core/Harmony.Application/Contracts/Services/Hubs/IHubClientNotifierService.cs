using Harmony.Application.DTO;
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
        Task UpdateCardDescription(Guid boardId, Guid cardId, string description);
        Task UpdateCardDates(Guid boardId, Guid cardId, DateTime? startDate, DateTime? dueDate);
        Task ToggleCardLabel(Guid boardId, Guid cardId, LabelDto label);
        Task AddCardAttachment(Guid boardId, Guid cardId, AttachmentDto attachment);
    }
}
