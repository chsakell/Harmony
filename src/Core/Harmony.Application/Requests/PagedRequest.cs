namespace Harmony.Application.Requests
{
    /// <summary>
    /// Base class for paginated requests
    /// </summary>
    public abstract class PagedRequest
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        public string[] OrderBy { get; set; }
    }
}
