
namespace Harmony.Domain.SourceControl
{
    public class Commit
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Url { get; set; }
        public Author Author { get; set; }
    }
}
