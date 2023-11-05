using Harmony.Application.Responses;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Boards.Queries.GetBoardUsers
{
    /// <summary>
    /// Response for getting a board's users
    /// </summary>
    public class UserBoardResponse : UserResponse
    {
        public bool IsMember { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public UserBoardAccess Access { get; set; }
    }
}
