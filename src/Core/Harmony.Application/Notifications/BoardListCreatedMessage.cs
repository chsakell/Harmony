using Harmony.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Notifications
{
    public class BoardListCreatedMessage
    {
        public Guid BoardId { get; set; }
        public BoardListDto BoardList {  get; set; }
    }
}
