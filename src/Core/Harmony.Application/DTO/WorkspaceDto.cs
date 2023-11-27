namespace Harmony.Application.DTO
{
    public class WorkspaceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<BoardDto> Boards { get; set; }
    }
}
