using System.ComponentModel.DataAnnotations;

namespace Site_2024.Models.Requests.RefundRequests
{
    public class RefundRequestAddRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int PartId { get; set; }

        public long? ShopifyOrderId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Reason { get; set; }

        [StringLength(4000)]
        public string Notes { get; set; }
    }
}