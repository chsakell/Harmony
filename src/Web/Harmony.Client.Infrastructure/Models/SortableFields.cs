
namespace Harmony.Client.Infrastructure.Models
{
    public class SortableFields
    {
        public List<SortableField> Fields { get; set; } = new List<SortableField>();

        public string[] BuildQueryString()
        {
            var queryParams = new List<string>();

            foreach (var field in Fields.Where(f => f.Enabled).OrderBy(f => f.Order))
            {
                queryParams.Add($"{field.Field} {(field.Descending ? "descending" : "ascending")}");
            }

            return queryParams.ToArray();
        }
    }

    public class SortableField
    {
        public string Field { get; set; }
        public string Label { get; set; }
        public int Order { get; set; }
        public bool Descending { get; set; }
        public bool Enabled { get; set; }
    }
}
