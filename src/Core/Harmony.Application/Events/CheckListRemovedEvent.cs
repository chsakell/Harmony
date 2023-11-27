namespace Harmony.Application.Events
{
    public class CheckListRemovedEvent
    {
        public Guid CheckListId { get; set; }
        public Guid CardId { get; set; }
        public int TotalItems { get; set; }
        public int TotalItemsCompleted { get; set; }
        public CheckListRemovedEvent(Guid checkListId, Guid cardId, int totalItems, int totalItemsCompleted)
        {
            CheckListId = checkListId;
            CardId = cardId;
            TotalItems = totalItems;
            TotalItemsCompleted = totalItemsCompleted;
        }
    }
}
