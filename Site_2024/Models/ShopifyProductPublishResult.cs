namespace Site_2024.Web.Api.Models.Shopify
{
    public class ShopifyProductPublishResult
    {
        public int PartId { get; set; }
        public string ProductGid { get; set; } = string.Empty;
        public long ProductId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
