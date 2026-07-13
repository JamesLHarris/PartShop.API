namespace Site_2024.Web.Api.Models
{
    public class ShopifyCheckoutResult
    {
        public string CheckoutUrl { get; set; } = string.Empty;
        public List<ShopifyCheckoutLineItemResult> Items { get; set; } = new();
    }
}
