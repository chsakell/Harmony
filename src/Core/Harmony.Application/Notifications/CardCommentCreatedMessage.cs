namespace Harmony.Application.Notifications
{
    public class CardCommentCreatedMessage
    {
        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
    }
}
