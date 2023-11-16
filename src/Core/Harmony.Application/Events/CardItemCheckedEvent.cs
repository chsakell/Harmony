namespace Harmony.Application.Events
{
    public class CardItemCheckedEvent
    {
        public int CardId { get; set; }
        public Guid CheckListItemId { get; set; }
        public bool IsChecked { get; set; }

        public CardItemCheckedEvent(int cardId, Guid checkListItemId, bool isChecked)
        {
            CardId = cardId;
            CheckListItemId = checkListItemId;
            IsChecked = isChecked;
        }
    }
}
