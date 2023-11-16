namespace Harmony.Application.Features.Cards.Commands.RemoveUserCard
{
    public class RemoveUserCardResponse 
    {
        public RemoveUserCardResponse(int cardId, string userId)
        {
            CardId = cardId;
            UserId = userId;
        }

        public int CardId { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
