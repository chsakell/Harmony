namespace Harmony.Application.DTO
{
    public class RetrospectiveDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid BoardId { get; set; }
        public Guid ParentBoardId { get; set; }
    }
}
