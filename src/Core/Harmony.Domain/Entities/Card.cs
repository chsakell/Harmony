﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Class to represent cards in lists
    /// </summary>
    public class Card : AuditableEntity<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public BoardList BoardList { get; set; }
        public Guid BoardListId { get; set; }
        public byte Position { get; set; } // position on the board list
    }
}
