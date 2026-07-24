namespace Site_2024.Web.Api.Models.Shopify
{
    public class ShopifyProductPublishResult
    {
        public int PartId { get; set; }
        public string ProductGid { get; set; } = string.Empty;
        public long ProductId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OnlineStorePublicationGid { get; set; } = string.Empty;
        public bool PublishedToOnlineStore { get; set; }
        public int InventoryQuantity { get; set; }
        public int ImagesRequested { get; set; }
        public int ImagesAdded { get; set; }
        public int ImagesSkipped { get; set; }
    }
}
