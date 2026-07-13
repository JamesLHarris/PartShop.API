namespace Site_2024.Web.Api.Models
{
    public class ShopifyCheckoutLineItemResult
    {
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string? PartNumber { get; set; }
        public int Quantity { get; set; }
        public long ShopifyVariantId { get; set; }
        public decimal Price { get; set; }
    }
}
