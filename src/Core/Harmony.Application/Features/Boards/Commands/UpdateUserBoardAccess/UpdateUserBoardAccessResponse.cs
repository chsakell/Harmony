using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Boards.Commands.UpdateUserBoardAccess
{
    /// <summary>
    /// Response for assigning access level to a board member
    /// </summary>
    public class UpdateUserBoardAccessResponse
    {
        public Guid BoardId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public UserBoardAccess Access { get; set; }
    }
}
