namespace Harmony.Application.Notifications
{
    public class CheckListRemovedMessage
    {
        public CheckListRemovedMessage(Guid boardId, Guid checkListId, Guid cardId, int totalItems, int totalItemsCompleted)
        {
            BoardId = boardId;
            CheckListId = checkListId;
            CardId = cardId;
            TotalItems = totalItems;
            TotalItemsCompleted = totalItemsCompleted;
        }

        public Guid BoardId { get; set; }
        public Guid CheckListId { get; set; }
        public Guid CardId { get; set; }
        public int TotalItems { get; set; }
        public int TotalItemsCompleted { get; set; }
    }
}
