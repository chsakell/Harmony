using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Represents a user's board
    /// </summary>
    public class Board : AuditableEntity<Guid>
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public BoardVisibility Visibility { get; set; }
        public List<BoardList> Lists { get; set; }
    }
}
