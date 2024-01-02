namespace Harmony.Application.DTO
{
    public class BurnDownReportDto
    {
        public string Name { get; set; }
        public List<string> Dates { get; set; }
        public List<double> GuideLineStoryPoints { get; set; }
        public List<double> RemainingStoryPoints { get; set; }
    }
}
