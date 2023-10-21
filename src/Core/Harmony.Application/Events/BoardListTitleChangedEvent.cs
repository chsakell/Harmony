namespace Harmony.Application.Events
{
    public class BoardListTitleChangedEvent
    {
        public Guid BoardId { get; set; }
        public Guid BoardListId { get; set; }
        public string Title { get; set; }

        public BoardListTitleChangedEvent(Guid boardId, Guid boardListId, string title)
        {
            BoardId = boardId;
            BoardListId = boardListId;
            Title = title;
        }
    }
}
