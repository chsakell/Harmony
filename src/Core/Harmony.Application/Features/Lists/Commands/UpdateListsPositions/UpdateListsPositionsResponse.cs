using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Lists.Commands.UpdateListsPositions
{
    public class UpdateListsPositionsResponse
    {
        public Guid BoardId { get; set; }
        public Dictionary<Guid, short> ListPositions { get; set; }

        public UpdateListsPositionsResponse(Guid boardId, Dictionary<Guid, short> listPositions)
        {
            BoardId = boardId;
            ListPositions = listPositions;
        }
    }
}
