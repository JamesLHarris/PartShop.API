namespace Site_2024.Web.Api.Models
{
    public class ShopifyDiscountDeactivateResult
    {
        public string DiscountGid { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? EndsAt { get; set; }
    }
}