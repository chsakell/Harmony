using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Intermidiate table to represent M2M relationship between cards and labels
    /// </summary>
    public class CardLabel
    {
        public Card Card { get; set; }
        public Guid CardId { get; set; }
        public BoardLabel BoardLabel { get; set; }
        public Guid BoardLabelId { get; set; }
    }
}
