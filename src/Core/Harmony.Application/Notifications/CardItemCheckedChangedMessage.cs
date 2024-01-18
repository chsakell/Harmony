namespace Harmony.Application.Notifications
{
    public class CardItemCheckedChangedMessage
    {
        public CardItemCheckedChangedMessage(Guid boardId, Guid cardId, Guid checkListItemId, bool isChecked)
        {
            BoardId = boardId;
            CardId = cardId;
            CheckListItemId = checkListItemId;
            IsChecked = isChecked;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public Guid CheckListItemId { get; set; }
        public bool IsChecked { get; set; }
    }
}
