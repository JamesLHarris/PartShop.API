namespace Site_2024.Web.Api.Models.Shopify
{
    public class ShopifyProductMediaSyncResult
    {
        public int PartId { get; set; }
        public string ProductGid { get; set; } = string.Empty;
        public int ImagesRequested { get; set; }
        public int ImagesAdded { get; set; }
        public int ImagesSkipped { get; set; }
    }
}
