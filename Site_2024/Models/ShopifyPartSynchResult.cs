namespace Site_2024.Web.Api.Models
{
    public class ShopifyPartSyncResult
    {
        public int PartId { get; set; }
        public ShopifyCreateProductResult CreateResult { get; set; }
        public ShopifyProductInventorySyncResult SyncResult { get; set; }
    }
}