namespace Site_2024.Models.Requests.RefundRequests
{
    public class RefundRequestSearchRequest 
    {
        public string Status { get; set; }
        public int? PartId { get; set; }
        public long? ShopifyOrderId { get; set; }
    }
}
