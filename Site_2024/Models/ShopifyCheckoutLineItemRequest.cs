namespace Site_2024.Web.Api.Models
{
    public class ShopifyCheckoutLineItemRequest
    {
        public int PartId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
