using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class AdminDiscountCodeShopifyCreatedRequest
    {
        [Required]
        [StringLength(200)]
        public string ShopifyDiscountGid { get; set; } = string.Empty;
    }
}
