using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Models.Board
{
    public class OrderedBoardListModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public short Position { get; set; }
    }
}
