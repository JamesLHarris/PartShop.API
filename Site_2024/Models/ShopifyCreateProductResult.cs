namespace Site_2024.Web.Api.Models
{
    public class ShopifyCreateProductResult
    {
        public string ProductGid { get; set; } = string.Empty;
        public string VariantGid { get; set; } = string.Empty;
        public string InventoryItemGid { get; set; } = string.Empty;

        public long ProductId { get; set; }
        public long VariantId { get; set; }
        public long InventoryItemId { get; set; }
    }
}