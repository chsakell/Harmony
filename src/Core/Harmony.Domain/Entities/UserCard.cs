namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Class to represent M 2 M relationship between users and cards
    /// (intermediate table)
    /// </summary>
    public class UserCard
    {
        public string UserId { get; set; }
        public Card Card { get; set; }
        public Guid CardId { get; set; }
    }
}
