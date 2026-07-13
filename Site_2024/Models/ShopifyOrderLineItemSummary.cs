namespace Site_2024.Web.Api.Models
{
    public class ShopifyOrderLineItemSummary
    {
        public string ShopifyLineItemGid { get; set; } = string.Empty;
        public long ShopifyLineItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? CurrencyCode { get; set; }
        public string? ShopifyImageUrl { get; set; }
        public string? ShopifyVariantGid { get; set; }
        public long? ShopifyVariantId { get; set; }
        public string? ShopifyProductGid { get; set; }
        public long? ShopifyProductId { get; set; }

        public ShopifyLocalPartMatch? LocalPart { get; set; }
    }
}
