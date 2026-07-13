namespace Site_2024.Web.Api.Models
{
    public class ShopifyOrderSyncLineItemResult
    {
        public string OrderName { get; set; } = string.Empty;
        public long ShopifyOrderId { get; set; }
        public long ShopifyLineItemId { get; set; }
        public long? ShopifyVariantId { get; set; }
        public int? PartId { get; set; }
        public string? PartName { get; set; }
        public int QuantityPurchased { get; set; }
        public bool WasSynced { get; set; }
        public bool WasAlreadySynced { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
