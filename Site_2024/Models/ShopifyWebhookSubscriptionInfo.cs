namespace Site_2024.Web.Api.Models
{
    public class ShopifyWebhookSubscriptionInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string? Format { get; set; }
    }
}
