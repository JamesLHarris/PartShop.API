namespace Site_2024.Web.Api.Models
{
    public class ShopifyWebhookProcessingResult
    {
        public bool IsHmacValid { get; set; }
        public bool IsDuplicate { get; set; }
        public string? WebhookId { get; set; }
        public string? Topic { get; set; }
        public string? ShopDomain { get; set; }
        public long ShopifyOrderId { get; set; }
        public string? OrderName { get; set; }
        public string? CheckoutToken { get; set; }
        public int LineItemsChecked { get; set; }
        public int PartsMarkedSold { get; set; }
        public int AlreadySyncedCount { get; set; }
        public int SkippedCount { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ShopifyWebhookLineItemResult> Items { get; set; } = new List<ShopifyWebhookLineItemResult>();
    }
}
