using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Entities
{
    public class Attachment : AuditableEntity<Guid>
    {
        public string FileName { get; set; }
        public string Location { get; set; }
        public Card Card { get; set; }
        public Guid CardId { get; set; }
    }
}
