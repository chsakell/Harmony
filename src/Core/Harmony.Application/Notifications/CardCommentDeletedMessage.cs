namespace Harmony.Application.Notifications
{
    public class CardCommentDeletedMessage
    {
        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
    }
}
