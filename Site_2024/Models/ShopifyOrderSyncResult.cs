namespace Site_2024.Web.Api.Models
{
    public class ShopifyOrderSyncResult
    {
        public int OrdersChecked { get; set; }
        public int LineItemsChecked { get; set; }
        public int PartsMarkedSold { get; set; }
        public int AlreadySyncedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<ShopifyOrderSyncLineItemResult> Items { get; set; } = new List<ShopifyOrderSyncLineItemResult>();
    }
}
