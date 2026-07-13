using Site_2024.Web.Api.Models;

namespace Site_2024.Web.Api.Services
{
    public interface IShopifyWebhookSubscriptionService
    {
        Task<ShopifyWebhookSubscriptionResult> RegisterOrdersPaidWebhookAsync(string callbackUrl);
        Task<List<ShopifyWebhookSubscriptionInfo>> GetOrderWebhookSubscriptionsAsync();
    }
}
