namespace Site_2024.Web.Api.Models
{
    public class ShopifyCheckoutRequest
    {
        public List<ShopifyCheckoutLineItemRequest> Items { get; set; } = new();
    }
}
