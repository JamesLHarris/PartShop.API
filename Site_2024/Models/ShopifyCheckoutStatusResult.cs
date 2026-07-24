namespace Site_2024.Web.Api.Models
{
    public class ShopifyCheckoutStatusResult
    {
        public string CheckoutToken { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public long? ShopifyOrderId { get; set; }
        public string? OrderName { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}
