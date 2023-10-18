using Harmony.Application.Responses;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Boards.Queries.GetBoardUsers
{
    public class UserBoardResponse : UserResponse
    {
        public bool IsMember { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public UserBoardAccess Access { get; set; }
    }
}
