namespace Site_2024.Web.Api.Models
{
    public class ShopifyProductInventorySyncResult
    {
        public string ProductGid { get; set; } = string.Empty;
        public string VariantGid { get; set; } = string.Empty;
        public string InventoryItemGid { get; set; } = string.Empty;
        public string LocationGid { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public bool VariantUpdated { get; set; }
        public bool InventoryItemUpdated { get; set; }
        public bool InventoryActivated { get; set; }
    }
}