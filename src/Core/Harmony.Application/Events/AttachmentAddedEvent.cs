using Harmony.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Events
{
    public class AttachmentAddedEvent
    {
        public Guid CardId { get; set; }
        public AttachmentDto Attachment { get; set; }

        public AttachmentAddedEvent(Guid cardId, AttachmentDto attachment)
        {
            CardId = cardId;
            Attachment = attachment;
        }
    }
}
