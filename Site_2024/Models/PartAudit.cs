namespace Site_2024.Web.Api.Models
{
    public class PartAudit
    {
        public int Id { get; set; }
        public PartSlim Part { get; set; }
        public UserSlim User { get; set; }
        public string ChangeType { get; set; }
        public string ColumnName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime ChangedOn { get; set; }
    }
}
