namespace Site_2024.Web.Api.Models.Shopify
{
    public class PartPatchResult
    {
        public bool LocalUpdated { get; set; }
        public bool ShopifySyncAttempted { get; set; }
        public bool ShopifySyncSucceeded { get; set; }
        public int? ShopifyQuantity { get; set; }
        public string? Warning { get; set; }
    }
}
