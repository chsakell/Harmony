namespace Harmony.Application.Events
{
    public class BoardListsPositionsChangedEvent
    {
        public BoardListsPositionsChangedEvent(Guid boardId, Dictionary<Guid, short> listPositions)
        {
            BoardId = boardId;
            ListPositions = listPositions;
        }

        public Guid BoardId { get; set; }
        public Dictionary<Guid, short> ListPositions { get; set; }
    }
}
