namespace Harmony.Client.Shared.Components
{
    public partial class AppSearch
    {
        private string _searchText { get; set; }

        private async Task<IEnumerable<string>> Search(string value, CancellationToken token)
        {
            //the http endpoint does not return immediately. There is an artifical delay built-in
            return new List<string>() {  "item 1", "item 2"};
        }

        //private async Task Search(string searchText)
        //{

        //}
    }
}
