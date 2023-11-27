namespace Harmony.Application.Features.Labels.Commands.CreateCardLabel
{
    public class CreateCardLabelResponse
    {
        public Guid BoardId { get; set; }
        public Guid? CardId { get; set; }
        public Guid LabelId { get; set; }
        public string Color { get; set; }
        public string Title { get; set; }
    }
}
