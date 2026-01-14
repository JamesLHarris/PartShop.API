using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class PartAuditAddRequest
    {
        [Required]
        public int PartId { get; set; }
        [Required]
        public int ChangedByUserId { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string ChangeType { get; set; }
        [Required]
        [StringLength (128, MinimumLength = 2)]
        public string ColumnName { get; set; }
        [Required]
        [StringLength (4000, MinimumLength =2)]
        public string OldValue { get; set; }
        [Required]
        [StringLength(4000, MinimumLength = 2)]
        public string NewValue { get; set; }
    }
}
