namespace Harmony.Application.Features.Lists.Commands.UpdateListTitle
{
    public class UpdateListTitleResponse
    {
        public UpdateListTitleResponse(Guid boardId, Guid listId, string title)
        {
            BoardId = boardId;
            ListId = listId;
            Title = title;
        }

        public Guid BoardId { get; set; }
        public Guid ListId { get; set; }
        public string Title { get; set; }
    }
}
