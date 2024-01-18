namespace Harmony.Application.Notifications
{
    public class BoardListsPositionsChangedMessage
    {
        public BoardListsPositionsChangedMessage(Guid boardId, Dictionary<Guid, short> listPositions)
        {
            BoardId = boardId;
            ListPositions = listPositions;
        }

        public Guid BoardId { get; set; }
        public Dictionary<Guid, short> ListPositions { get; set; }
    }
}
