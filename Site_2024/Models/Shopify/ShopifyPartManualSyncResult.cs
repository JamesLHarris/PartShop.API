namespace Site_2024.Web.Api.Models.Shopify
{
    public class ShopifyPartManualSyncResult
    {
        public int PartId { get; set; }
        public ShopifyProductInventorySyncResult Inventory { get; set; } = new ShopifyProductInventorySyncResult();
        public ShopifyProductMediaSyncResult Media { get; set; } = new ShopifyProductMediaSyncResult();
    }
}
