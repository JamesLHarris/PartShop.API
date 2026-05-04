using System.ComponentModel.DataAnnotations;

namespace Site_2024.Models.Requests.RefundRequests
{
    public class RefundRequestUpdateStatusRequest
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(4000)]
        public string Notes { get; set; }
    }
}