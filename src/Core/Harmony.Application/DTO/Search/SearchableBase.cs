namespace Harmony.Application.DTO.Search
{
    public abstract class SearchableBase
    {
        public string ObjectId { get; set; }

        public SearchableBase(Guid cardId)
        {
            ObjectId = cardId.ToString();
        }
    }
}
