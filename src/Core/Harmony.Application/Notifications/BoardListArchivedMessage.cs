namespace Harmony.Application.Notifications
{
    public class BoardListArchivedMessage
    {
        public Guid BoardId { get; set; }
        public Guid ArchivedList { get; set; }

        public List<BoardListOrder> Positions { get; set; }
        public BoardListArchivedMessage(Guid boardId,
            Guid archivedList,
            List<BoardListOrder> positions)
        {
            BoardId = boardId;
            ArchivedList = archivedList;
            Positions = positions;
        }

        public class BoardListOrder
        {
            public BoardListOrder(Guid id, short position)
            {
                Id = id;
                Position = position;
            }

            public Guid Id { get; set; }
            public short Position { get; set; }
        }
    }
}
