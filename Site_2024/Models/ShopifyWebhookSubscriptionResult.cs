namespace Site_2024.Web.Api.Models
{
    public class ShopifyWebhookSubscriptionResult
    {
        public ShopifyWebhookSubscriptionInfo? Subscription { get; set; }
        public List<string> UserErrors { get; set; } = new List<string>();
    }
}
