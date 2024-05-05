
namespace Harmony.Client.Infrastructure.Models
{
    public class SortableFields
    {
        public IEnumerable<SortableField> Fields { get; set; } = new List<SortableField>();

        public int MaxOrder => Fields.Select(x => x.Order).Max();

        public string[] BuildQueryString()
        {
            var queryParams = new List<string>();

            foreach (var field in Fields.Where(f => f.Enabled).OrderBy(f => f.Order))
            {
                queryParams.Add($"{field.Field} {(field.Descending ? "descending" : "ascending")}");
            }

            return queryParams.ToArray();
        }

        public void MoveDown(SortableField field)
        {
            var swapField = Fields.FirstOrDefault(f => f.Order == field.Order + 1);

            if (swapField != null)
            {
                field.Order += 1;
                swapField.Order -= 1;
            }
        }

        public void MoveUp(SortableField field)
        {
            var swapField = Fields.FirstOrDefault(f => f.Order == field.Order - 1);

            if (swapField != null)
            {
                field.Order -= 1;
                swapField.Order += 1;
            }
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
