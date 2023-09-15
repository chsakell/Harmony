using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Represents a card's activity
    /// </summary>
    public class CardActivity : AuditableEntity<Guid>
    {
        public string Activity { get; set; }
        public Card Card { get; set; }
        public Guid CardId { get; set; }
        public string UserId { get; set; }
    }
}
