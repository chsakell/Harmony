namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Class to represent a check list item in a check list
    /// </summary>
    public class CheckListItem : AuditableEntity<Guid>
    {
        public string Description { get; set; }
        public CheckList CheckList { get; set; }
        public Guid CheckListId { get; set; }
        public bool IsChecked { get; set; }
        public byte Position { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
