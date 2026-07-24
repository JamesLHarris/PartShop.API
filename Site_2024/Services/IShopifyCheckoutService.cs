using Site_2024.Web.Api.Models;

namespace Site_2024.Web.Api.Services
{
    public interface IShopifyCheckoutService
    {
        ShopifyCheckoutResult CreateCheckoutUrl(
            ShopifyCheckoutRequest request);

        ShopifyCheckoutStatusResult? GetCheckoutStatus(
            string checkoutToken);
    }
}
