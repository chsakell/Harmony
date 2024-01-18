namespace Harmony.Application.Notifications
{
    public class BoardListTitleChangedMessage
    {
        public Guid BoardId { get; set; }
        public Guid BoardListId { get; set; }
        public string Title { get; set; }

        public BoardListTitleChangedMessage(Guid boardId, Guid boardListId, string title)
        {
            BoardId = boardId;
            BoardListId = boardListId;
            Title = title;
        }
    }
}
