using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Class to represent M 2 M relationship between users and cards
    /// (intermediate table)
    /// </summary>
    public class UserCard
    {
        public string UserId { get; set; }
        public Card Card { get; set; }
        public Guid CardId { get; set; }
    }
}
