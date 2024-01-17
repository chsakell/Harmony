namespace Harmony.Application.Notifications
{
    public class CardTitleChangedMessage
    {
        public CardTitleChangedMessage(Guid boardId, Guid cardId, string title)
        {
            BoardId = boardId;
            CardId = cardId;
            Title = title;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public string Title { get; set; }
    }
}
