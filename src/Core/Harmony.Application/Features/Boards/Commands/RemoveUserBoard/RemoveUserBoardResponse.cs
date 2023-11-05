namespace Harmony.Application.Features.Boards.Commands.RemoveUserBoard
{
    /// <summary>
    /// Response for board member removal
    /// </summary>
    public class RemoveUserBoardResponse 
    {
        public RemoveUserBoardResponse(Guid boardId, string userId)
        {
            BoardId = boardId;
            UserId = userId;
        }

        public Guid BoardId { get; set; }
        public string UserId { get; set; }
    }
}
