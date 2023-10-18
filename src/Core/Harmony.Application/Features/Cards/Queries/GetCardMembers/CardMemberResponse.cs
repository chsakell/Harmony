using Harmony.Application.Responses;

namespace Harmony.Application.Features.Cards.Queries.GetCardMembers
{
    public class CardMemberResponse : UserResponse
    {
        public bool IsMember { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
