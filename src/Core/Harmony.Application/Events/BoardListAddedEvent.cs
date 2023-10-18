using Harmony.Application.DTO;

namespace Harmony.Application.Events
{
    public class BoardListAddedEvent
    {
        public BoardListDto BoardList { get; set; }

        public BoardListAddedEvent(BoardListDto boardList)
        {
            BoardList = boardList;
        }
    }
}
