using Harmony.Application.DTO;

namespace Harmony.Application.Notifications
{
    public class BoardListCreatedMessage
    {
        public Guid BoardId { get; set; }
        public BoardListDto BoardList {  get; set; }
    }
}
